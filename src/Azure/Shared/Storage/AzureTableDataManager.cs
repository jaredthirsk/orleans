using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Forkleans.Internal;
using Forkleans.Runtime;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

//
// Number of #ifs can be reduced (or removed), once we separate test projects by feature/area, otherwise we are ending up with ambigous types and build errors.
//

#if ORLEANS_CLUSTERING
namespace Forkleans.Clustering.AzureStorage
#elif ORLEANS_PERSISTENCE
namespace Forkleans.Persistence.AzureStorage
#elif ORLEANS_REMINDERS
namespace Forkleans.Reminders.AzureStorage
#elif ORLEANS_STREAMING
namespace Forkleans.Streaming.AzureStorage
#elif ORLEANS_EVENTHUBS
namespace Forkleans.Streaming.EventHubs
#elif TESTER_AZUREUTILS
namespace Forkleans.Tests.AzureUtils
#elif ORLEANS_TRANSACTIONS
namespace Forkleans.Transactions.AzureStorage
#elif ORLEANS_DIRECTORY
namespace Forkleans.GrainDirectory.AzureStorage
#else
// No default namespace intentionally to cause compile errors if something is not defined
#endif
{
    /// <summary>
    /// Utility class to encapsulate row-based access to Azure table storage.
    /// </summary>
    /// <typeparam name="T">Table data entry used by this table / manager.</typeparam>
    internal partial class AzureTableDataManager<T> where T : class, ITableEntity
    {
        private readonly AzureStorageOperationOptions options;

        /// <summary> Name of the table this instance is managing. </summary>
        public string TableName { get; }

        /// <summary> Logger for this table manager instance. </summary>
        protected internal ILogger Logger { get; }

        public AzureStoragePolicyOptions StoragePolicyOptions { get; }

        public TableClient Table { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AzureTableDataManager{T}"/> instance.
        /// </summary>
        /// <param name="options">Storage configuration.</param>
        /// <param name="logger">Logger to use.</param>
        public AzureTableDataManager(AzureStorageOperationOptions options, ILogger logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            TableName = options.TableName ?? throw new ArgumentNullException(nameof(options.TableName));
            StoragePolicyOptions = options.StoragePolicyOptions ?? throw new ArgumentNullException(nameof(options.StoragePolicyOptions));

            AzureTableUtils.ValidateTableName(TableName);
        }

        /// <summary>
        /// Connects to, or creates and initializes a new Azure table if it does not already exist.
        /// </summary>
        /// <returns>Completion promise for this operation.</returns>
        public async Task InitTableAsync()
        {
            const string operation = "InitTable";
            var startTime = DateTime.UtcNow;

            try
            {
                TableServiceClient tableCreationClient = await GetCloudTableCreationClientAsync();
                var table = tableCreationClient.GetTableClient(TableName);
                var tableItem = await table.CreateIfNotExistsAsync();
                var didCreate = tableItem is not null;

                LogInfoTableCreation(Logger, didCreate ? "Created" : "Attached to", TableName);
                Table = table;
            }
            catch (TimeoutException te)
            {
                LogErrorTableCreationInTimeout(Logger, te, StoragePolicyOptions.CreationTimeout);
                throw new ForkleansException($"Unable to create or connect to the Azure table in {StoragePolicyOptions.CreationTimeout}", te);
            }
            catch (Exception exc)
            {
                LogErrorTableCreation(Logger, exc, TableName);
                throw;
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Deletes the Azure table.
        /// </summary>
        /// <returns>Completion promise for this operation.</returns>
        public async Task DeleteTableAsync()
        {
            const string operation = "DeleteTable";
            var startTime = DateTime.UtcNow;

            try
            {
                var tableCreationClient = await GetCloudTableCreationClientAsync();
                var response = await tableCreationClient.DeleteTableAsync(TableName);
                if (response.Status == 204)
                {
                    LogInfoTableDeletion(Logger, TableName);
                }
            }
            catch (Exception exc)
            {
                LogErrorTableDeletion(Logger, exc, TableName);
                throw;
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Deletes all entities the Azure table.
        /// </summary>
        /// <returns>Completion promise for this operation.</returns>
        public async Task ClearTableAsync()
        {
            var items = await ReadAllTableEntriesAsync();
            IEnumerable<Task> work = items.GroupBy(item => item.Item1.PartitionKey)
                                          .SelectMany(partition => partition.ToBatch(this.StoragePolicyOptions.MaxBulkUpdateRows))
                                          .Select(batch => DeleteTableEntriesAsync(batch.ToList()));
            await Task.WhenAll(work);
        }

        /// <summary>
        /// Create a new data entry in the Azure table (insert new, not update existing).
        /// Fails if the data already exists.
        /// </summary>
        /// <param name="data">Data to be inserted into the table.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        public async Task<string> CreateTableEntryAsync(T data)
        {
            const string operation = "CreateTableEntry";
            var startTime = DateTime.UtcNow;

            LogTraceTableEntryCreation(Logger, TableName, data);
            try
            {
                try
                {
                    // Presumably FromAsync(BeginExecute, EndExecute) has a slightly better performance then CreateIfNotExistsAsync.
                    var opResult = await Table.AddEntityAsync(data);
                    return opResult.Headers.ETag.GetValueOrDefault().ToString();
                }
                catch (Exception exc)
                {
                    CheckAlertWriteError(operation, data, null, exc);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Inserts a data entry in the Azure table: creates a new one if does not exists or overwrites (without eTag) an already existing version (the "update in place" semantics).
        /// </summary>
        /// <param name="data">Data to be inserted or replaced in the table.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        public async Task<string> UpsertTableEntryAsync(T data)
        {
            const string operation = "UpsertTableEntry";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntry(Logger, operation, data, TableName);
            try
            {
                try
                {
                    var opResult = await Table.UpsertEntityAsync(data);
                    return opResult.Headers.ETag.GetValueOrDefault().ToString();
                }
                catch (Exception exc)
                {
                    LogWarningUpsertTableEntry(Logger, exc, data, TableName);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Inserts a data entry in the Azure table: creates a new one if does not exists
        /// </summary>
        /// <param name="data">Data to be inserted or replaced in the table.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        public async Task<(bool isSuccess, string eTag)> InsertTableEntryAsync(T data)
        {
            const string operation = "InsertTableEntry";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntry(Logger, operation, data, TableName);
            try
            {
                try
                {
                    var opResult = await Table.AddEntityAsync(data);
                    return (true, opResult.Headers.ETag.GetValueOrDefault().ToString());
                }
                catch (RequestFailedException storageException) when (storageException.Status == (int)HttpStatusCode.Conflict)
                {
                    return (false, null);
                }
                catch (Exception exc)
                {
                    LogWarningInsertTableEntry(Logger, exc, data, TableName);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Merges a data entry in the Azure table.
        /// </summary>
        /// <param name="data">Data to be merged in the table.</param>
        /// <param name="eTag">ETag to apply.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        internal Task<string> MergeTableEntryAsync(T data, string eTag) => MergeTableEntryAsync(data, new ETag(eTag));

        /// <summary>
        /// Merges a data entry in the Azure table.
        /// </summary>
        /// <param name="data">Data to be merged in the table.</param>
        /// <param name="eTag">ETag to apply.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        internal async Task<string> MergeTableEntryAsync(T data, ETag eTag)
        {
            const string operation = "MergeTableEntry";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntry(Logger, operation, data, TableName);
            try
            {
                try
                {
                    // Merge requires an ETag (which may be the '*' wildcard).
                    data.ETag = eTag;
                    var opResult = await Table.UpdateEntityAsync(data, data.ETag, TableUpdateMode.Merge);
                    return opResult.Headers.ETag.GetValueOrDefault().ToString();
                }
                catch (Exception exc)
                {
                    LogWarningMergeTableEntry(Logger, exc, data, TableName);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Updates a data entry in the Azure table: updates an already existing data in the table, by using eTag.
        /// Fails if the data does not already exist or of eTag does not match.
        /// </summary>
        /// <param name="data">Data to be updated into the table.</param>
        /// /// <param name="dataEtag">ETag to use.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        public Task<string> UpdateTableEntryAsync(T data, string dataEtag) => UpdateTableEntryAsync(data, new ETag(dataEtag));

        /// <summary>
        /// Updates a data entry in the Azure table: updates an already existing data in the table, by using eTag.
        /// Fails if the data does not already exist or of eTag does not match.
        /// </summary>
        /// <param name="data">Data to be updated into the table.</param>
        /// /// <param name="dataEtag">ETag to use.</param>
        /// <returns>Value promise with new Etag for this data entry after completing this storage operation.</returns>
        public async Task<string> UpdateTableEntryAsync(T data, ETag dataEtag)
        {
            const string operation = "UpdateTableEntryAsync";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntry(Logger, operation, data, TableName);

            try
            {
                try
                {
                    data.ETag = dataEtag;
                    var opResult = await Table.UpdateEntityAsync(data, data.ETag, TableUpdateMode.Replace);

                    //The ETag of data is needed in further operations.
                    return opResult.Headers.ETag.GetValueOrDefault().ToString();
                }
                catch (Exception exc)
                {
                    CheckAlertWriteError(operation, data, null, exc);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Deletes an already existing data in the table, by using eTag.
        /// Fails if the data does not already exist or if eTag does not match.
        /// </summary>
        /// <param name="data">Data entry to be deleted from the table.</param>
        /// <param name="eTag">ETag to use.</param>
        /// <returns>Completion promise for this storage operation.</returns>
        public Task DeleteTableEntryAsync(T data, string eTag) => DeleteTableEntryAsync(data, new ETag(eTag));

        /// <summary>
        /// Deletes an already existing data in the table, by using eTag.
        /// Fails if the data does not already exist or if eTag does not match.
        /// </summary>
        /// <param name="data">Data entry to be deleted from the table.</param>
        /// <param name="eTag">ETag to use.</param>
        /// <returns>Completion promise for this storage operation.</returns>
        public async Task DeleteTableEntryAsync(T data, ETag eTag)
        {
            const string operation = "DeleteTableEntryAsync";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntry(Logger, operation, data, TableName);
            try
            {
                data.ETag = eTag;
                try
                {
                    var response = await Table.DeleteEntityAsync(data.PartitionKey, data.RowKey, data.ETag);
                    if (response is { Status: 404 })
                    {
                        throw new RequestFailedException(response.Status, "Resource not found", response.ReasonPhrase, null);
                    }
                }
                catch (Exception exc)
                {
                    LogWarningDeleteTableEntry(Logger, exc, data, TableName);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Read a single table entry from the storage table.
        /// </summary>
        /// <param name="partitionKey">The partition key for the entry.</param>
        /// <param name="rowKey">The row key for the entry.</param>
        /// <returns>Value promise for tuple containing the data entry and its corresponding etag.</returns>
        public async Task<(T Entity, string ETag)> ReadSingleTableEntryAsync(string partitionKey, string rowKey)
        {
            const string operation = "ReadSingleTableEntryAsync";
            var startTime = DateTime.UtcNow;
            LogTraceTableOperation(Logger, operation, TableName, partitionKey, rowKey);
            try
            {
                try
                {
                    var result = await Table.GetEntityIfExistsAsync<T>(partitionKey, rowKey);
                    if (result.HasValue)
                    {
                        //The ETag of data is needed in further operations.
                        return (result.Value, result.Value.ETag.ToString());
                    }
                }
                catch (RequestFailedException exception)
                {
                    if (!AzureTableUtils.TableStorageDataNotFound(exception))
                    {
                        throw;
                    }
                }

                LogDebugTableEntryNotFound(Logger, partitionKey, rowKey);
                return (default, default);  // No data
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Read all entries in one partition of the storage table.
        /// NOTE: This could be an expensive and slow operation for large table partitions!
        /// </summary>
        /// <param name="partitionKey">The key for the partition to be searched.</param>
        /// <returns>Enumeration of all entries in the specified table partition.</returns>
        public Task<List<(T Entity, string ETag)>> ReadAllTableEntriesForPartitionAsync(string partitionKey)
        {
            string query = TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}");
            return ReadTableEntriesAndEtagsAsync(query);
        }

        /// <summary>
        /// Read all entries in the table.
        /// NOTE: This could be a very expensive and slow operation for large tables!
        /// </summary>
        /// <returns>Enumeration of all entries in the table.</returns>
        public Task<List<(T Entity, string ETag)>> ReadAllTableEntriesAsync()
        {
            return ReadTableEntriesAndEtagsAsync(null);
        }

        /// <summary>
        /// Deletes a set of already existing data entries in the table, by using eTag.
        /// Fails if the data does not already exist or if eTag does not match.
        /// </summary>
        /// <param name="collection">Data entries and their corresponding etags to be deleted from the table.</param>
        /// <returns>Completion promise for this storage operation.</returns>
        public async Task DeleteTableEntriesAsync(List<(T Entity, string ETag)> collection)
        {
            const string operation = "DeleteTableEntries";
            var startTime = DateTime.UtcNow;
            LogTraceTableEntries(Logger, operation, new(collection), TableName);

            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection.Count > this.StoragePolicyOptions.MaxBulkUpdateRows)
            {
                throw new ArgumentOutOfRangeException(nameof(collection), collection.Count,
                        "Too many rows for bulk delete - max " + this.StoragePolicyOptions.MaxBulkUpdateRows);
            }

            if (collection.Count == 0)
            {
                return;
            }

            try
            {
                var entityBatch = new List<TableTransactionAction>();
                foreach (var tuple in collection)
                {
                    T item = tuple.Entity;
                    item.ETag = new ETag(tuple.ETag);
                    entityBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete, item, item.ETag));
                }

                try
                {
                    _ = await Table.SubmitTransactionAsync(entityBatch);
                }
                catch (Exception exc)
                {
                    LogWarningDeleteTableEntries(Logger, exc, new(collection), TableName);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Read data entries and their corresponding eTags from the Azure table.
        /// </summary>
        /// <param name="filter">Filter string to use for querying the table and filtering the results.</param>
        /// <returns>Enumeration of entries in the table which match the query condition.</returns>
        public async Task<List<(T Entity, string ETag)>> ReadTableEntriesAndEtagsAsync(string filter)
        {
            const string operation = "ReadTableEntriesAndEtags";
            var startTime = DateTime.UtcNow;

            try
            {
                try
                {
                    async Task<List<(T Entity, string ETag)>> executeQueryHandleContinuations()
                    {
                        var list = new List<(T, string)>();
                        var results = Table.QueryAsync<T>(filter);
                        await foreach (var value in results)
                        {
                            list.Add((value, value.ETag.ToString()));
                        }

                        return list;
                    }

#if !ORLEANS_TRANSACTIONS
                    IBackoffProvider backoff = new FixedBackoff(this.StoragePolicyOptions.PauseBetweenOperationRetries);

                    List<(T, string)> results = await AsyncExecutorWithRetries.ExecuteWithRetries(
                        counter => executeQueryHandleContinuations(),
                        this.StoragePolicyOptions.MaxOperationRetries,
                        (exc, counter) => AzureTableUtils.AnalyzeReadException(exc.GetBaseException(), counter, TableName, Logger),
                        this.StoragePolicyOptions.OperationTimeout,
                        backoff);
#else
                    List<(T, string)> results = await executeQueryHandleContinuations();
#endif
                    // Data was read successfully if we got to here
                    return results;

                }
                catch (Exception exc)
                {
                    // Out of retries...
                    if (!AzureTableUtils.TableStorageDataNotFound(exc))
                    {
                        LogWarningReadTable(Logger, exc, TableName);
                    }

                    throw new ForkleansException($"Failed to read Azure Storage table {TableName}", exc);
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        /// <summary>
        /// Inserts a set of new data entries into the table.
        /// Fails if the data does already exists.
        /// </summary>
        /// <param name="collection">Data entries to be inserted into the table.</param>
        /// <returns>Completion promise for this storage operation.</returns>
        public async Task BulkInsertTableEntries(IReadOnlyCollection<T> collection)
        {
            const string operation = "BulkInsertTableEntries";
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (collection.Count > this.StoragePolicyOptions.MaxBulkUpdateRows)
            {
                throw new ArgumentOutOfRangeException(nameof(collection), collection.Count,
                        "Too many rows for bulk update - max " + this.StoragePolicyOptions.MaxBulkUpdateRows);
            }

            if (collection.Count == 0)
            {
                return;
            }

            var startTime = DateTime.UtcNow;
            LogTraceTableEntriesCount(Logger, operation, collection.Count, TableName);
            try
            {
                var entityBatch = new List<TableTransactionAction>(collection.Count);
                foreach (T entry in collection)
                {
                    entityBatch.Add(new TableTransactionAction(TableTransactionActionType.Add, entry));
                }

                try
                {
                    await Table.SubmitTransactionAsync(entityBatch);
                }
                catch (Exception exc)
                {
                    LogWarningBulkInsertTableEntries(Logger, exc, collection.Count, TableName);
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        internal async Task<(string, string)> InsertTwoTableEntriesConditionallyAsync(T data1, T data2, string data2Etag)
        {
            const string operation = "InsertTableEntryConditionally";
            string data2Str = data2 == null ? "null" : data2.ToString();
            var startTime = DateTime.UtcNow;

            LogTraceTableEntries(Logger, operation, data1, data2Str, TableName);
            try
            {
                try
                {
                    data2.ETag = new ETag(data2Etag);
                    var opResults = await Table.SubmitTransactionAsync(new TableTransactionAction[]
                    {
                        new TableTransactionAction(TableTransactionActionType.Add, data1),
                        new TableTransactionAction(TableTransactionActionType.UpdateReplace, data2, data2.ETag)
                    });

                    //The batch results are returned in order of execution,
                    //see reference at https://msdn.microsoft.com/en-us/library/microsoft.windowsazure.storage.table.cloudtable.executebatch.aspx.
                    //The ETag of data is needed in further operations.
                    var resultETag1 = opResults.Value[0].Headers.ETag.GetValueOrDefault().ToString();
                    var resultETag2 = opResults.Value[1].Headers.ETag.GetValueOrDefault().ToString();
                    return (resultETag1, resultETag2);
                }
                catch (Exception exc)
                {
                    CheckAlertWriteError(operation, data1, data2Str, exc);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        internal async Task<(string, string)> UpdateTwoTableEntriesConditionallyAsync(T data1, string data1Etag, T data2, string data2Etag)
        {
            const string operation = "UpdateTableEntryConditionally";
            string data2Str = data2 == null ? "null" : data2.ToString();
            var startTime = DateTime.UtcNow;
            LogTraceTableEntries(Logger, operation, data1, data2Str, TableName);

            try
            {
                try
                {
                    data1.ETag = new ETag(data1Etag);
                    var entityBatch = new List<TableTransactionAction>(2);
                    entityBatch.Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, data1, data1.ETag));

                    if (data2 != null && data2Etag != null)
                    {
                        data2.ETag = new ETag(data2Etag);
                        entityBatch.Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, data2, data2.ETag));
                    }

                    var opResults = await Table.SubmitTransactionAsync(entityBatch);

                    //The batch results are returned in order of execution,
                    //see reference at https://msdn.microsoft.com/en-us/library/microsoft.windowsazure.storage.table.cloudtable.executebatch.aspx.
                    //The ETag of data is needed in further operations.
                    var resultETag1 = opResults.Value[0].Headers.ETag.GetValueOrDefault().ToString();
                    var resultETag2 = opResults.Value[1].Headers.ETag.GetValueOrDefault().ToString();
                    return (resultETag1, resultETag2);
                }
                catch (Exception exc)
                {
                    CheckAlertWriteError(operation, data1, data2Str, exc);
                    throw;
                }
            }
            finally
            {
                CheckAlertSlowAccess(startTime, operation);
            }
        }

        private async ValueTask<TableServiceClient> GetCloudTableCreationClientAsync()
        {
            try
            {
                return await options.CreateClient();
            }
            catch (Exception exc)
            {
                LogErrorTableServiceClientCreation(Logger, exc);
                throw;
            }
        }

        private void CheckAlertWriteError(string operation, object data1, string data2, Exception exc)
        {
            HttpStatusCode httpStatusCode;
            if (AzureTableUtils.EvaluateException(exc, out httpStatusCode, out _) && AzureTableUtils.IsContentionError(httpStatusCode))
            {
                // log at Verbose, since failure on conditional is not not an error. Will analyze and warn later, if required.
                LogWarningTableWrite(Logger, operation, TableName, data1 ?? "null", data2 ?? "null");
            }
            else
            {
                LogErrorTableWrite(Logger, exc, operation, TableName, data1);
            }
        }

        private void CheckAlertSlowAccess(DateTime startOperation, string operation)
        {
            var timeSpan = DateTime.UtcNow - startOperation;
            if (timeSpan > this.StoragePolicyOptions.OperationTimeout)
            {
                LogWarningSlowAccess(Logger, operation, TableName, timeSpan);
            }
        }

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = (int)Utilities.ErrorCode.AzureTable_01,
            Message = "{Action} Azure storage table {TableName}"
        )]
        private static partial void LogInfoTableCreation(ILogger logger, string action, string tableName);

        [LoggerMessage(
            Level = LogLevel.Error,
            EventId = (int)Utilities.ErrorCode.AzureTable_TableNotCreated,
            Message = "Unable to create or connect to the Azure table in {CreationTimeout}"
        )]
        private static partial void LogErrorTableCreationInTimeout(ILogger logger, TimeoutException exception, TimeSpan creationTimeout);

        [LoggerMessage(
            Level = LogLevel.Error,
            EventId = (int)Utilities.ErrorCode.AzureTable_02,
            Message = "Could not initialize connection to storage table {TableName}"
        )]
        private static partial void LogErrorTableCreation(ILogger logger, Exception exception, string tableName);

        [LoggerMessage(
            Level = LogLevel.Information,
            EventId = (int)Utilities.ErrorCode.AzureTable_03,
            Message = "Deleted Azure storage table {TableName}"
        )]
        private static partial void LogInfoTableDeletion(ILogger logger, string tableName);

        [LoggerMessage(
            Level = LogLevel.Error,
            EventId = (int)Utilities.ErrorCode.AzureTable_04,
            Message = "Could not delete storage table {TableName}"
        )]
        private static partial void LogErrorTableDeletion(ILogger logger, Exception exception, string tableName);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "Creating {TableName} table entry: {Data}"
        )]
        private static partial void LogTraceTableEntryCreation(ILogger logger, string tableName, T data);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "{Operation} entry {Data} into table {TableName}"
        )]
        private static partial void LogTraceTableEntry(ILogger logger, string operation, T data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_06,
            Message = "Intermediate error upserting entry {Data} to the table {TableName}"
        )]
        private static partial void LogWarningUpsertTableEntry(ILogger logger, Exception exception, T data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_06,
            Message = "Intermediate error inserting entry {Data} to the table {TableName}"
        )]
        private static partial void LogWarningInsertTableEntry(ILogger logger, Exception exception, T data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_07,
            Message = "Intermediate error merging entry {Data} to the table {TableName}"
        )]
        private static partial void LogWarningMergeTableEntry(ILogger logger, Exception exception, T data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_08,
            Message = "Intermediate error deleting entry {Data} from the table {TableName}."
        )]
        private static partial void LogWarningDeleteTableEntry(ILogger logger, Exception exception, T data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "{Operation} table {TableName} partitionKey {PartitionKey} rowKey {RowKey}"
        )]
        private static partial void LogTraceTableOperation(ILogger logger, string operation, string tableName, string partitionKey, string rowKey);

        [LoggerMessage(
            Level = LogLevel.Debug,
            Message = "Could not find table entry for PartitionKey={PartitionKey} RowKey={RowKey}"
        )]
        private static partial void LogDebugTableEntryNotFound(ILogger logger, string partitionKey, string rowKey);

        private readonly struct CollectionLogEntry(List<(T Entity, string ETag)> collection)
        {
            public override string ToString() => Utils.EnumerableToString(collection);
        }

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "{Operation} entries: {Data} table {TableName}"
        )]
        private static partial void LogTraceTableEntries(ILogger logger, string operation, CollectionLogEntry data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_08,
            Message = "Intermediate error deleting entries {Data} from the table {TableName}."
        )]
        private static partial void LogWarningDeleteTableEntries(ILogger logger, Exception exception, CollectionLogEntry data, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_09,
            Message = "Failed to read Azure Storage table {TableName}"
        )]
        private static partial void LogWarningReadTable(ILogger logger, Exception exception, string tableName);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "{Operation} {Count} entries table {TableName}"
        )]
        private static partial void LogTraceTableEntriesCount(ILogger logger, string operation, int count, string tableName);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_37,
            Message = "Intermediate error bulk inserting {Count} entries in the table {TableName}"
        )]
        private static partial void LogWarningBulkInsertTableEntries(ILogger logger, Exception exception, int count, string tableName);

        [LoggerMessage(
            Level = LogLevel.Trace,
            Message = "{Operation} data1 {Data1} data2 {Data2} table {TableName}"
        )]
        private static partial void LogTraceTableEntries(ILogger logger, string operation, T data1, string data2, string tableName);

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "Error creating TableServiceClient."
        )]
        private static partial void LogErrorTableServiceClientCreation(ILogger logger, Exception exception);

        [LoggerMessage(
            Level = LogLevel.Debug,
            EventId = (int)Utilities.ErrorCode.AzureTable_13,
            Message = "Intermediate Azure table write error {Operation} to table {TableName} data1 {Data1} data2 {Data2}"
        )]
        private static partial void LogWarningTableWrite(ILogger logger, string operation, string tableName, object data1, object data2);

        [LoggerMessage(
            Level = LogLevel.Error,
            EventId = (int)Utilities.ErrorCode.AzureTable_14,
            Message = "Azure table access write error {Operation} to table {TableName} entry {Data1}"
        )]
        private static partial void LogErrorTableWrite(ILogger logger, Exception exception, string operation, string tableName, object data1);

        [LoggerMessage(
            Level = LogLevel.Warning,
            EventId = (int)Utilities.ErrorCode.AzureTable_15,
            Message = "Slow access to Azure Table {TableName} for {Operation}, which took {Duration}"
        )]
        private static partial void LogWarningSlowAccess(ILogger logger, string tableName, string operation, TimeSpan duration);
    }

    internal static class TableDataManagerInternalExtensions
    {
        internal static IEnumerable<IEnumerable<TItem>> ToBatch<TItem>(this IEnumerable<TItem> source, int size)
        {
            using (IEnumerator<TItem> enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return Take(enumerator, size);
        }

        private static IEnumerable<TItem> Take<TItem>(IEnumerator<TItem> source, int size)
        {
            int i = 0;
            do
                yield return source.Current;
            while (++i < size && source.MoveNext());
        }
    }
}
