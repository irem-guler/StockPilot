using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public class ReorderSuggestion
    {
        public Product Product { get; set; } = null!;

        public int CurrentStock { get; set; }

        public int SuggestedQuantity { get; set; }
    }

    public interface IReorderService
    {
        Task<List<ReorderSuggestion>> GetSuggestionsAsync();
    }
}