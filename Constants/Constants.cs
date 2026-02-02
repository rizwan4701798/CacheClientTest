namespace TestApp.Constants;

public static class ClientTestConstants
{
    // General
    public const string AppTitle = "Cache Server Test Application";
    public const string DefaultClientName = "Default";
    public const string UnknownClient = "Unknown";
    
    // Config
    public const string Localhost = "localhost";
    public const int DefaultPort = 5050;
    public const int DefaultTimeout = 5000;

    // Messages - Init/Shutdown
    public const string InitFailed = "Failed to initialize cache client. Ensure the cache server is running.";
    public const string InitSuccess = "Cache client initialized successfully!";
    public const string Initializing = "Initializing cache client...";
    public const string InitError = "Initialization failed: {0}";
    public const string Disposed = "Cache client disposed. Goodbye!";
    public const string PressAnyKey = "Press any key to continue...";

    // Headers
    public const string MainMenuHeader = "MAIN MENU";
    public const string CrudHeader = "CRUD OPERATIONS";
    public const string ExpirationHeader = "EXPIRATION TESTS";
    public const string EventHeader = "EVENT NOTIFICATION TESTS";
    public const string PerfHeader = "PERFORMANCE BENCHMARKS";
    public const string StressHeader = "STRESS TESTS";
    public const string InteractiveHeader = "INTERACTIVE MODE";
    public const string MultiClientHeader = "MULTI-CLIENT MANAGER";

    // Menu Options
    public const string Option1 = "1";
    public const string Option2 = "2";
    public const string Option3 = "3";
    public const string Option4 = "4";
    public const string Option5 = "5";
    public const string Option6 = "6";
    public const string Option7 = "7";
    public const string OptionQ = "Q";
    public const string Option0 = "0";
    public const string OptionB = "B";

    // Menu Descriptions
    public const string DescCrud = "Manual CRUD Operations";
    public const string DescExpiration = "Expiration Tests";
    public const string DescEvents = "Event Notification Tests";
    public const string DescPerf = "Performance Benchmarks";
    public const string DescStress = "Stress Tests";
    public const string DescInteractive = "Interactive Mode";
    public const string DescMultiClient = "Multi-Client Manager";
    public const string DescQuit = "Quit";
    
    // CRUD Menu
    public const string DescAdd = "Add Product";
    public const string DescGet = "Get Product";
    public const string DescUpdate = "Update Product";
    public const string DescRemove = "Remove Product";
    public const string DescAddEx = "Add with Expiration";
    public const string DescBack = "Back to Main Menu";

    // Multi-Client Menu
    public const string DescCreateClient = "Create New Client";
    public const string DescSwitchClient = "Switch Current Client";
    public const string DescListClients = "List Active Clients";
    public const string DescBroadcast = "Broadcast Message";
    public const string DescBulkCreate = "Bulk Create Clients";
    public const string DescBulkAdd = "Bulk Add Operation";

    // Stress Menu
    public const string DescStressWriters = "Concurrent Writers Test";
    public const string DescStressReaders = "Concurrent Readers Test";
    public const string DescStressMixed = "Mixed Concurrent Operations";
    public const string DescStressRapid = "Rapid Fire Test";

    // Prompts
    public const string PromptSelectOption = "Select option";
    public const string InvalidChoice = "Invalid choice. Please try again.";
    public const string InvalidChoiceShort = "Invalid choice.";
    public const string ErrorPrefix = "Error: {0}";
    public const string OperationFailed = "Operation failed: {0}";
    
    // Keys & Values
    public const string KeyProduct1 = "product:1";
    public const string NameSample = "Sample Product";
    public const string DescSample = "A sample product";
    public const string Price99 = "99.99";
    public const string Price49 = "49.99";
    public const string Price149 = "149.99";
    public const string UpdatedProduct = "Updated Product";
    
    // CRUD Messages
    public const string AddHeader = "Add New Product";
    public const string PromptKey = "Enter cache key (e.g., product:1)";
    public const string PromptName = "Enter product name";
    public const string PromptPrice = "Enter price";
    public const string PromptDesc = "Enter description";
    public const string AddedSuccess = "Product '{0}' added with key '{1}'";
    
    public const string AddExHeader = "Add Product with Expiration";
    public const string PromptKeySimple = "Enter cache key";
    public const string PromptExSeconds = "Enter expiration in seconds";
    public const string TempProduct = "Temporary Product";
    public const string AddedExSuccess = "Product added with {0}s expiration";
    
    public const string GetHeader = "Get Product";
    public const string ProductFound = "Product found:";
    public const string ValueFound = "Value found (type: {0}): {1}";
    public const string ProductNotFound = "Product not found in cache.";
    
    public const string UpdateHeader = "Update Product";
    public const string PromptNewName = "Enter new product name";
    public const string PromptNewPrice = "Enter new price";
    public const string UpdateDesc = "Updated at {0:HH:mm:ss}";
    public const string UpdatedSuccess = "Product updated successfully";
    
    public const string RemoveHeader = "Remove Product";
    public const string RemovedSuccess = "Product with key '{0}' removed";

    // Expiration Messages
    public const string ExpTestInfo = "This test will add items with different expiration times and verify they expire correctly.";
    public const string MonitoringExp = "Monitoring expiration...";
    public const string StopMonitoring = "Press any key to stop monitoring.\n";
    public const string ExpAdded = "Added '{0}' with {1}s expiration";
    public const string ExpCompleted = "Expiration test completed in {0:mm\\:ss}";
    
    // Event Messages
    public const string Subscribing = "Subscribing to cache events...";
    public const string SubscribedAll = "Subscribed to all cache events!";
    public const string EventWatchInfo = "Now performing operations. Watch for event notifications:";
    public const string AddingItem = "  -> Adding item...";
    public const string UpdatingItem = "  -> Updating item...";
    public const string RemovingItem = "  -> Removing item...";
    public const string AddingExItem = "  -> Adding item with short expiration (2s)...";
    public const string WaitingForExp = "  -> Waiting for expiration...";
    public const string EventTestCompleted = "Event notification test completed!";
    public const string RealTimeEvents = "Note: Events are displayed in real-time as they occur.";
    public const string EventTestFailed = "Event test failed: {0}";
    
    // Performance Messages
    public const string PromptIterations = "Enter number of iterations (default: 1000)";
    public const string RunWriteBench = "Running WRITE benchmark ({0} iterations)...";
    public const string RunReadBench = "Running READ benchmark ({0} iterations)...";
    public const string RunUpdateBench = "Running UPDATE benchmark ({0} iterations)...";
    public const string RunMixedBench = "Running MIXED benchmark ({0} iterations)...";
    public const string CleaningBench = "Cleaning up benchmark data...";
    public const string PerfCompleted = "Performance benchmarks completed!";
    
    // Stress Messages
    public const string PromptThreads10 = "Enter number of threads (default: 10)";
    public const string PromptThreads15 = "Enter number of threads (default: 15)";
    public const string PromptThreads20 = "Enter number of threads (default: 20)";
    public const string PromptOps100 = "Enter operations per thread (default: 100)";
    public const string PromptOps200 = "Enter operations per thread (default: 200)";
    public const string PromptOps500 = "Enter operations per thread (default: 500)";
    public const string PromptTotalOps = "Enter total operations (default: 5000)";
    public const string PromptSelectTest = "Select test";
    
    public const string RunWriters = "Running {0} concurrent writer threads, {1} ops each...";
    public const string RunReaders = "Running {0} concurrent reader threads, {1} ops each...";
    public const string PrePopulating = "Pre-populating cache with test data...";
    public const string RunMixed = "Running {0} concurrent mixed operation threads, {1} ops each...";
    public const string RunRapid = "Running rapid fire test with {0} sequential operations...";
    public const string CleaningUp = "Cleaning up...";
    public const string StressHitMiss = "  Cache hits: {0}, Misses: {1}";
    public const string StressMixedStats = "  Writes: {0}, Reads: {1}, Updates: {2}, Deletes: {3}";
    public const string StressCompleted = "Stress test completed!";
    
    // Interactive
    public const string InteractiveInfo = "Enter commands directly. Type 'help' for available commands, 'exit' to return.";
    public const string AvailableCommands = "Available commands:";
    public const string UnknownCommand = "Unknown command: {0}. Type 'help' for available commands.";
    public const string UsageAdd = "Usage: add <key> <value>";
    public const string UsageAddEx = "Usage: addex <key> <ttl_seconds> <value>";
    public const string UsageGet = "Usage: get <key>";
    public const string UsageUpdate = "Usage: update <key> <value>";
    public const string UsageDel = "Usage: del <key>";
    public const string OK = "OK";
    public const string OKExpires = "OK (expires in {0}s)";
    public const string Nil = "(nil)";
    
    // Multi-Client
    public const string PromptClientName = "Enter unique client name";
    public const string PromptNameEmpty = "Name cannot be empty.";
    public const string PromptClientExists = "Client '{0}' already exists."; // Updated to match format
    public const string PromptClientExistsSimple = "Client with this name already exists.";
    public const string ClientCreated = "Client '{0}' created and connected.";
    public const string ClientCreateFailed = "Failed to create client '{0}': {1}";
    public const string ClientCreateFailedSimple = "Failed to create client: {0}";
    
    public const string PromptUseClient = "Enter client name to use";
    public const string SwitchedToClient = "Switched to client '{0}'.";
    public const string ClientNotFound = "Client not found.";
    public const string ActiveClients = "Active Clients:";
    
    public const string PromptMessage = "Enter message";
    public const string BroadcastInfo = "Broadcasting using all {0} clients...";
    public const string ClientSentUpdate = "Client {0} sent update.";
    public const string ClientFailed = "Client {0} failed: {1}";
    
    public const string PromptBulkCount = "Enter number of clients to create";
    public const string PromptPrefix = "Enter name prefix (e.g. 'LoadClient')";
    public const string InvalidCount = "Invalid count.";
    public const string BulkCreating = "Creating {0} clients...";
    public const string BulkComplete = "Bulk creation complete.";
    
    public const string PromptKeyPrefix = "Enter key prefix (e.g. 'bulk:item')";
    public const string SendingAddRequests = "Sending ADD requests from all {0} clients...";
    public const string BulkAddComplete = "Bulk Add Completed. Success: {0}, Failed: {1}";
    
    // Events
    public const string ADDED = "ADDED";
    public const string UPDATED = "UPDATED";
    public const string REMOVED = "REMOVED";
    public const string EXPIRED = "EXPIRED";
    public const string EVICTED = "EVICTED";
}
