using System.Text.Json;
using OpenAI.Chat;

namespace StockPilot.Web.Services
{
    public class AssistantService
    {
        private readonly AssistantDataService _data;
        private readonly IConfiguration _configuration;

        public AssistantService(
            AssistantDataService data,
            IConfiguration configuration)
        {
            _data = data;
            _configuration = configuration;
        }

        public async Task<string> AskAsync(string userQuestion)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "The assistant is not configured. Please set the OpenAI API key.";
            }

            var client = new ChatClient("gpt-4o-mini", apiKey);


            var tools = new List<ChatTool>
            {
                ChatTool.CreateFunctionTool(
                    "get_inventory_summary",
                    "Get a general summary of the inventory: total products, warehouses, units in stock, and critical item count."),

                ChatTool.CreateFunctionTool(
                    "get_critical_stock",
                    "Get the list of products that are at or below their reorder level (critical stock)."),

                ChatTool.CreateFunctionTool(
                    "search_product",
                    "Search for a product by name or SKU and get its stock across warehouses.",
                    BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "query": { "type": "string", "description": "Product name or SKU to search for" }
                        },
                        "required": ["query"]
                    }
                    """)),

                ChatTool.CreateFunctionTool(
                    "get_warehouse_stock",
                    "Get the stock status of a specific warehouse by its name.",
                    BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "warehouseName": { "type": "string", "description": "Name of the warehouse" }
                        },
                        "required": ["warehouseName"]
                    }
                    """)),

                ChatTool.CreateFunctionTool(
                    "get_top_selling",
                    "Get the list of top selling products by quantity sold."),

                ChatTool.CreateFunctionTool(
                    "get_sales_summary",
                    "Get a summary of sales orders: counts, pending, and monthly value."),

                ChatTool.CreateFunctionTool(
                    "get_inventory_value",
                    "Get the total inventory value and ABC classification summary."),

                ChatTool.CreateFunctionTool(
                    "get_transfer_suggestions",
                    "Get suggested stock transfers between warehouses. For each warehouse that has critical stock, suggests transferring from the nearest warehouse that has enough of that product."),

                ChatTool.CreateFunctionTool(
                    "get_transfer_history",
                    "Get the history of stock transfers between warehouses, showing which routes had the most movement.")
            };

            var options = new ChatCompletionOptions();
            foreach (var tool in tools)
            {
                options.Tools.Add(tool);
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a helpful assistant for StockPilot, an inventory management system. " +
                    "Answer questions about inventory, stock, warehouses, sales and product values. " +
                    "Use the provided tools to fetch real data before answering. " +
                    "Keep answers concise and clear. If the data does not contain the answer, say so honestly. " +
                    "Answer in the same language the user used."),
                new UserChatMessage(userQuestion)
            };


            for (int turn = 0; turn < 5; turn++)
            {
                ChatCompletion completion = await client.CompleteChatAsync(messages, options);

                if (completion.FinishReason == ChatFinishReason.ToolCalls)
                {

                    messages.Add(new AssistantChatMessage(completion));

                    foreach (var toolCall in completion.ToolCalls)
                    {
                        var result = await ExecuteToolAsync(toolCall);
                        messages.Add(new ToolChatMessage(toolCall.Id, result));
                    }

                    continue;
                }

                return completion.Content.Count > 0
                    ? completion.Content[0].Text
                    : "I couldn't generate a response.";
            }

            return "The request was too complex to complete. Please try rephrasing.";
        }

        private async Task<string> ExecuteToolAsync(ChatToolCall toolCall)
        {
            try
            {
                switch (toolCall.FunctionName)
                {
                    case "get_inventory_summary":
                        return await _data.GetInventorySummaryAsync();

                    case "get_critical_stock":
                        return await _data.GetCriticalStockAsync();

                    case "get_top_selling":
                        return await _data.GetTopSellingAsync();

                    case "get_sales_summary":
                        return await _data.GetSalesSummaryAsync();

                    case "get_inventory_value":
                        return await _data.GetInventoryValueAsync();

                    case "search_product":
                        {
                            var args = JsonDocument.Parse(toolCall.FunctionArguments);
                            var query = args.RootElement.GetProperty("query").GetString() ?? "";
                            return await _data.SearchProductAsync(query);
                        }

                    case "get_warehouse_stock":
                        {
                            var args = JsonDocument.Parse(toolCall.FunctionArguments);
                            var name = args.RootElement.GetProperty("warehouseName").GetString() ?? "";
                            return await _data.GetWarehouseStockAsync(name);
                        }

                    case "get_transfer_suggestions":
                        return await _data.GetTransferSuggestionsAsync();

                    case "get_transfer_history":
                        return await _data.GetTransferHistoryAsync();

                    default:
                        return "Unknown function.";
                }
            }
            catch (Exception ex)
            {
                return $"Error running the function: {ex.Message}";
            }
        }
    }
}