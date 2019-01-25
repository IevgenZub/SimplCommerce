using System.Collections.Generic;

namespace SimplCommerce.Infrastructure.Web.SmartTable
{
    public class SmartTableResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();

        public int TotalRecord { get; set; }

        public int NumberOfPages { get; set; }
    }
}
