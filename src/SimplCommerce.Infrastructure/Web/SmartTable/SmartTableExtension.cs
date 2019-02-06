﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace SimplCommerce.Infrastructure.Web.SmartTable
{
    public static class SmartTableExtension
    {
        public static SmartTableResult<TResult> ToSmartTableResult<TModel, TResult>(this IQueryable<TModel> query, SmartTableParam param, Expression<Func<TModel, TResult>> selector)
        {
            if (param.Pagination.Number <= 0)
            {
                param.Pagination.Number = 10;
            }

            var totalRecord = query.Count();

            if (!string.IsNullOrWhiteSpace(param.Sort.Predicate))
            {
                query = query.OrderByName(param.Sort.Predicate, param.Sort.Reverse);
            }
            else
            {
                 query = query.OrderByName("Id", true);
            }

            var items = query
                .Skip(param.Pagination.Start)
                .Take(param.Pagination.Number)
                .Select(selector).ToList();

            return new SmartTableResult<TResult>
            {
                Items = items,
                TotalRecord = totalRecord,
                NumberOfPages = (int)Math.Ceiling((double)totalRecord / param.Pagination.Number)
            };
        }
        public static SmartTableResult<TResult> ToSmartTableResult<TModel, TResult>(this IQueryable<TModel> query, SmartTableParam param, Expression<Func<TModel, TResult>> selector, Expression<Func<TModel, object>> sortExpression)
        {
            if (param.Pagination.Number <= 0)
            {
                param.Pagination.Number = 10;
            }

            var totalRecord = query.Count();

            if (!string.IsNullOrWhiteSpace(param.Sort.Predicate))
            {
                if (param.Sort.Reverse)
                {
                    query = query.OrderBy(sortExpression);
                }
                else
                {
                    query = query.OrderByDescending(sortExpression);
                }
            }
            else
            {
                query = query.OrderByName("Id", true);
            }

            var items = query
                .Skip(param.Pagination.Start)
                .Take(param.Pagination.Number)
                .Select(selector).ToList();

            return new SmartTableResult<TResult>
            {
                Items = items,
                TotalRecord = totalRecord,
                NumberOfPages = (int)Math.Ceiling((double)totalRecord / param.Pagination.Number)
            };
        }

    }
}
