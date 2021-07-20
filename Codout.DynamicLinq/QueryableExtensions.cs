﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Codout.DynamicLinq
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Applies data processing (paging, sorting and filtering) over IQueryable using Dynamic Linq.
        /// </summary>
        /// <typeparam name="T">The type of the IQueryable.</typeparam>
        /// <param name="queryable">The IQueryable which should be processed.</param>
        /// <param name="take">Specifies how many items to take. Configurable via the pageSize setting of the Kendo DataSource.</param>
        /// <param name="skip">Specifies how many items to skip.</param>
        /// <param name="sort">Specifies the current sort order.</param>
        /// <param name="filter">Specifies the current filter.</param>
        /// <returns>A DataSourceResult object populated from the processed IQueryable.</returns>
        public static DataSourceResult ToDataSourceResult<T>(this IQueryable<T> queryable, int take, int skip, IEnumerable<Sort> sort, Filter filter)
        {
            return queryable.ToDataSourceResult(take, skip, sort, filter, null, null);
        }

        /// <summary>
        ///  Applies data processing (paging, sorting and filtering) over IQueryable using Dynamic Linq.
        /// </summary>
        /// <typeparam name="T">The type of the IQueryable.</typeparam>
        /// <param name="queryable">The IQueryable which should be processed.</param>
        /// <param name="request">The DataSourceRequest object containing take, skip, sort, filter, aggregates, and groups data.</param>
        /// <returns>A DataSourceResult object populated from the processed IQueryable.</returns>
        public static DataSourceResult ToDataSourceResult<T>(this IQueryable<T> queryable, DataSourceRequest request)
        {
            return queryable.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter, request.Aggregate, request.Group);
        }

        /// <summary>
        /// Applies data processing (paging, sorting, filtering and aggregates) over IQueryable using Dynamic Linq.
        /// </summary>
        /// <typeparam name="T">The type of the IQueryable.</typeparam>
        /// <param name="queryable">The IQueryable which should be processed.</param>
        /// <param name="take">Specifies how many items to take. Configurable via the pageSize setting of the Kendo DataSource.</param>
        /// <param name="skip">Specifies how many items to skip.</param>
        /// <param name="sort">Specifies the current sort order.</param>
        /// <param name="filter">Specifies the current filter.</param>
        /// <param name="aggregates">Specifies the current aggregates.</param>
        /// <param name="group">Specifies the current groups.</param>
        /// <returns>A DataSourceResult object populated from the processed IQueryable.</returns>
        public static DataSourceResult ToDataSourceResult<T>(this IQueryable<T> queryable, int take, int skip, IEnumerable<Sort> sort, Filter filter, IEnumerable<Aggregator> aggregates, IEnumerable<Group> group)
        {
            var errors = new List<object>();

            // Filter the data first
            queryable = Filters(queryable, filter, errors);

            // Calculate the total number of records (needed for paging)            
            var total = queryable.Count();

            // Calculate the aggregates
            var aggregate = Aggregates(queryable, aggregates);

            if (group?.Any() == true)
            {
                //if(sort == null) sort = GetDefaultSort(queryable.ElementType, sort);
                if(sort == null) sort = new List<Sort>();

                foreach (var source in group.Reverse())
                {
                    sort = sort.Append(new Sort
                    {
                        Field = source.Field,
                        Dir = source.Dir
                    });
                }
            }

            // Sort the data
            queryable = Sort(queryable, sort);

            // Finally page the data
            if (take > 0)
            {
                queryable = Page(queryable, take, skip);
            }

            var result = new DataSourceResult
            {
                Total = total,
                Aggregates = aggregate
            };

            // Group By
            if (group?.Any() == true)
            {
                //result.Groups = queryable.ToList().GroupByMany(group);                
                result.Groups = queryable.GroupByMany(group);
            }
            else
            {
                result.Data = queryable.ToList();
            }

            // Set errors if any
            if(errors.Count > 0)
            {
                result.Errors = errors;
            }

            return result;
        }

        private static IQueryable<T> Filters<T>(IQueryable<T> queryable, Filter filter, List<object> errors)
        {
            if (filter?.Logic != null)
            {
                // Pretreatment some work
                filter = PreliminaryWork(typeof(T),filter);

                // Collect a flat list of all filters
                var filters = filter.All();

                /* Method.1 Use the combined expression string */
                // Step.1 Create a predicate expression e.g. Field1 = @0 And Field2 > @1
                string predicate;
                try
                {
                    predicate = filter.ToExpression(typeof(T), filters);
                }
                catch(Exception ex)
                {
                    errors.Add(ex.Message);
                    return queryable;
                }

                // Step.2 Get all filter values as array (needed by the Where method of Dynamic Linq)
                var values = filters.Select(f => f.Value).ToArray();

                // Step.3 Use the Where method of Dynamic Linq to filter the data
                queryable = queryable.Where(predicate, values);

                /* Method.2 Use the combined lambda expression */
                // Step.1 Create a parameter "p"
                //var parameter = Expression.Parameter(typeof(T), "p");

                // Step.2 Make up expression e.g. (p.Number >= 3) AndAlso (p.Company.Name.Contains("M"))
                //Expression expression;
                //try 
                //{
                //    expression = filter.ToLambdaExpression<T>(parameter, filters);         
                //}
                //catch(Exception ex)
                //{
                //    errors.Add(ex.Message);
                //    return queryable;
                //} 

                // Step.3 The result is e.g. p => (p.Number >= 3) AndAlso (p.Company.Name.Contains("M"))
                //var predicateExpression = Expression.Lambda<Func<T, bool>>(expression, parameter);
                //queryable = queryable.Where(predicateExpression);
            }

            return queryable;
        }

        internal static object Aggregates<T>(IQueryable<T> queryable, IEnumerable<Aggregator> aggregates)
        {
            if (aggregates?.Any() == true)
            {
                var objProps = new Dictionary<DynamicProperty, object>();
                var groups = aggregates.GroupBy(g => g.Field);
                Type type = null;

                foreach (var group in groups)
                {
                    var fieldProps = new Dictionary<DynamicProperty, object>();
                    foreach (var aggregate in group)
                    {
                        var prop = typeof(T).GetProperty(aggregate.Field);
                        var param = Expression.Parameter(typeof(T), "s");
                        var selector = aggregate.Aggregate == "count" && (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                            ? Expression.Lambda(Expression.NotEqual(Expression.MakeMemberAccess(param, prop), Expression.Constant(null, prop.PropertyType)), param)
                            : Expression.Lambda(Expression.MakeMemberAccess(param, prop), param);
                        var mi = aggregate.MethodInfo(typeof(T));
                        if (mi == null) continue;

                        var val = queryable.Provider.Execute(Expression.Call(null, mi, aggregate.Aggregate == "count" && (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                                  ? new[] { queryable.Expression }
                                  : new[] { queryable.Expression, Expression.Quote(selector) }));

                        fieldProps.Add(new DynamicProperty(aggregate.Aggregate, typeof(object)), val);
                    }

                    type = DynamicClassFactory.CreateType(fieldProps.Keys.ToList());
                    var fieldObj = Activator.CreateInstance(type);
                    foreach (var p in fieldProps.Keys)
                    {
                        type.GetProperty(p.Name).SetValue(fieldObj, fieldProps[p], null);
                    }
                    objProps.Add(new DynamicProperty(group.Key, fieldObj.GetType()), fieldObj);
                }

                type = DynamicClassFactory.CreateType(objProps.Keys.ToList());

                var obj = Activator.CreateInstance(type);
                foreach (var p in objProps.Keys)
                {
                    type.GetProperty(p.Name).SetValue(obj, objProps[p], null);
                }

                return obj;
            }

            return null;
        }

        private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
        {
            if (sort?.Any() == true)
            {
                // Create ordering expression e.g. Field1 asc, Field2 desc
                var ordering = string.Join(",", sort.Select(s => s.ToExpression()));

                // Use the OrderBy method of Dynamic Linq to sort the data
                return queryable.OrderBy(ordering);
            }

            return queryable;
        }

        private static IQueryable<T> Page<T>(IQueryable<T> queryable, int take, int skip)
        {
            return queryable.Skip(skip).Take(take);
        }

        /// <summary>
        /// Pretreatment of specific DateTime type and convert some illegal value type
        /// </summary>
        /// <param name="filter"></param>
        private static Filter PreliminaryWork(Type type, Filter filter)
        {
            if (filter.Filters != null && filter.Logic != null)
            {
                var newFilters = new List<Filter>();
                foreach (var f in filter.Filters)
                {
                    newFilters.Add(PreliminaryWork(type, f));
                }

                filter.Filters = newFilters;
            }

            if(filter.Value == null) return filter;

            // When we have a decimal value, it gets converted to an integer/double that will result in the query break
            var currentPropertyType = Filter.GetLastPropertyType(type, filter.Field);
            if((currentPropertyType == typeof(decimal)) && decimal.TryParse(filter.Value.ToString(), out decimal number))
            {
                filter.Value = number;
                return filter;
            }

            // if(currentPropertyType.GetTypeInfo().IsEnum && int.TryParse(filter.Value.ToString(), out int enumValue))
            // {           
            //     filter.Value = Enum.ToObject(currentPropertyType, enumValue);
            //     return filter;
            // }

            // Convert datetime-string to DateTime
            if(currentPropertyType == typeof(DateTime) && DateTime.TryParse(filter.Value.ToString(), out DateTime dateTime))
            {
                filter.Value = dateTime;

                // Copy the time from the filter
                var localTime = dateTime.ToLocalTime();

                // Used when the datetime's operator value is eq and local time is 00:00:00 
                if (filter.Operator == "eq")
                {
                    if (localTime.Hour != 0 || localTime.Minute != 0 || localTime.Second != 0)
                        return filter;

                    var newFilter = new Filter { Logic = "and"};
                    newFilter.Filters = new List<Filter>
                    {
                        // Instead of comparing for exact equality, we compare as greater than the start of the day...
                        new Filter
                        {
                            Field = filter.Field,
                            Filters = filter.Filters,
                            Value = new DateTime(localTime.Year, localTime.Month, localTime.Day, 0, 0, 0),
                            Operator = "gte"
                        },
                        // ...and less than the end of that same day (we're making an additional filter here)
                        new Filter
                        {
                            Field = filter.Field,
                            Filters = filter.Filters,
                            Value = new DateTime(localTime.Year, localTime.Month, localTime.Day, 23, 59, 59),
                            Operator = "lte"
                        }
                    };

                    return newFilter;
                }

                // Convert datetime to local 
                filter.Value = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, localTime.Minute, localTime.Second, localTime.Millisecond);
            }

            return filter;
        }

        /// <summary>
        /// The way this extension works it pages the records using skip and takes to do that we need at least one sort property.
        /// </summary>
        private static IEnumerable<Sort> GetDefaultSort(Type type, IEnumerable<Sort> sort)
        {
            if (sort == null)
            {
                var elementType = type;
                var properties = elementType.GetProperties().ToList();

                //by default make dir desc
                var sortByObject = new Sort
                {
                    Dir = "desc"
                };

                PropertyInfo propertyInfo;
                //look for property that is called id
                if (properties.Any(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)))
                {
                    propertyInfo = properties.FirstOrDefault(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase));
                }
                //or contains id
                else if (properties.Any(p => p.Name.IndexOf("id", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    propertyInfo = properties.FirstOrDefault(p => p.Name.IndexOf("id", StringComparison.OrdinalIgnoreCase) >= 0);
                }
                //or just get the first property
                else
                {
                    propertyInfo = properties.FirstOrDefault();
                }

                if (propertyInfo != null)
                {
                    sortByObject.Field = propertyInfo.Name;
                }
                sort = new List<Sort> { sortByObject };
            }

            return sort;
        }

    }
}
