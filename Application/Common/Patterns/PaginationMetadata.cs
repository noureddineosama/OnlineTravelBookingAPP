using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Patterns
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        private int _pageSize; //. good tips 
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(PageSize), "PageSize must be greater than zero.");
                else if (value > 50)
                    throw new ArgumentOutOfRangeException(nameof(PageSize), "PageSize must be lower than 50 items.");

                _pageSize = value;
            }
        }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public bool HasNextPage => CurrentPage < TotalPages; //. true --> has next page, false --> has not next page 
        public bool HasPreviousPage => CurrentPage > 1; //. true --> the current page is greater than 1 and has a previous page 
    }
}
