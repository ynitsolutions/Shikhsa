using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Common;

namespace Shikhsa.Helpers
{
    public static class EntityNameMapperExtensions
    {
        public static async Task PopulateLookupNamesAsync<T>(
            this T entity,
            ApplicationDbContext context)
            where T : class
        {
            if (entity == null)
                return;

            var entityType = typeof(T);

            var mappedProperties = entityType
                .GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttributes(
                        typeof(MapNameAttribute),
                        true)
                        .Cast<MapNameAttribute>()
                        .FirstOrDefault()
                })
                .Where(x => x.Attribute != null)
                .ToList();

            foreach (var item in mappedProperties)
            {
                var sourceProperty = item.Property;
                var attribute = item.Attribute!;

                var sourceValue = sourceProperty.GetValue(entity);

                if (sourceValue == null)
                    continue;

                var targetProperty = entityType.GetProperty(
                    attribute.NameProperty);

                if (targetProperty == null || !targetProperty.CanWrite)
                    continue;

                string? name = null;

                // -----------------------------------------
                // DataListItem lookup
                // -----------------------------------------
                if (!string.IsNullOrWhiteSpace(attribute.DataListName))
                {
                    var id = Convert.ToInt32(sourceValue);

                    name = await context.DataListItems
                        .AsNoTracking()
                        .Where(x =>
                            x.DataListItemId == id &&
                            x.DataList != null &&
                            x.DataList.DataListName ==
                                attribute.DataListName)
                        .Select(x => x.DataListItemText)
                        .FirstOrDefaultAsync();
                }

                // -----------------------------------------
                // Normal entity lookup
                // -----------------------------------------
                else if (attribute.LookupType != null &&
                         !string.IsNullOrWhiteSpace(
                             attribute.LookupKeyProperty) &&
                         !string.IsNullOrWhiteSpace(
                             attribute.LookupNameProperty))
                {
                    var id = Convert.ToInt32(sourceValue);

                    name = await GetEntityNameAsync(
                        context,
                        attribute.LookupType,
                        attribute.LookupKeyProperty!,
                        attribute.LookupNameProperty!,
                        id);
                }

                if (name != null)
                {
                    targetProperty.SetValue(entity, name);
                }
            }
        }


        private static async Task<string?> GetEntityNameAsync(
            ApplicationDbContext context,
            Type entityType,
            string keyProperty,
            string nameProperty,
            int id)
        {
            // DbContext.Set(Type) is not available in the
            // generic form we need, so invoke Set<TEntity>()
            // through reflection.

            var setMethod = typeof(DbContext)
                .GetMethods()
                .First(x =>
                    x.Name == nameof(DbContext.Set) &&
                    x.IsGenericMethod &&
                    x.GetParameters().Length == 0);

            var genericSetMethod =
                setMethod.MakeGenericMethod(entityType);

            var queryable =
                (IQueryable)genericSetMethod.Invoke(
                    context,
                    null)!;

            var parameter =
                System.Linq.Expressions.Expression.Parameter(
                    entityType,
                    "x");

            var keyExpression =
                System.Linq.Expressions.Expression.Property(
                    parameter,
                    keyProperty);

            var idExpression =
                System.Linq.Expressions.Expression.Constant(
                    id,
                    keyExpression.Type);

            var equalsExpression =
                System.Linq.Expressions.Expression.Equal(
                    keyExpression,
                    idExpression);

            var whereLambda =
                System.Linq.Expressions.Expression.Lambda(
                    equalsExpression,
                    parameter);

            var whereMethod =
                typeof(Queryable)
                    .GetMethods()
                    .First(x =>
                        x.Name == nameof(Queryable.Where) &&
                        x.IsGenericMethodDefinition &&
                        x.GetParameters().Length == 2 &&
                        x.GetParameters()[1]
                            .ParameterType
                            .GetGenericArguments()
                            .Length == 2);

            var genericWhereMethod =
                whereMethod.MakeGenericMethod(entityType);

            var filteredQuery =
                (IQueryable)genericWhereMethod.Invoke(
                    null,
                    new object[]
                    {
                        queryable,
                        whereLambda
                    })!;

            var namePropertyExpression =
                System.Linq.Expressions.Expression.Property(
                    parameter,
                    nameProperty);

            var selectLambda =
                System.Linq.Expressions.Expression.Lambda(
                    namePropertyExpression,
                    parameter);

            var selectMethod =
                typeof(Queryable)
                    .GetMethods()
                    .First(x =>
                        x.Name == nameof(Queryable.Select) &&
                        x.IsGenericMethodDefinition &&
                        x.GetParameters().Length == 2 &&
                        x.GetParameters()[1]
                            .ParameterType
                            .GetGenericArguments()
                            .Length == 2);

            var genericSelectMethod =
                selectMethod.MakeGenericMethod(
                    entityType,
                    namePropertyExpression.Type);

            var selectedQuery =
                genericSelectMethod.Invoke(
                    null,
                    new object[]
                    {
                        filteredQuery,
                        selectLambda
                    })!;

            var firstOrDefaultMethod =
                typeof(EntityFrameworkQueryableExtensions)
                    .GetMethods()
                    .First(x =>
                        x.Name == nameof(
                            EntityFrameworkQueryableExtensions
                                .FirstOrDefaultAsync) &&
                        x.IsGenericMethodDefinition &&
                        x.GetParameters().Length == 2);

            var genericFirstOrDefaultMethod =
                firstOrDefaultMethod.MakeGenericMethod(
                    namePropertyExpression.Type);

            var task =
                (Task)genericFirstOrDefaultMethod.Invoke(
                    null,
                    new object[]
                    {
                        selectedQuery,
                        null!
                    })!;

            await task;

            return task
                .GetType()
                .GetProperty("Result")?
                .GetValue(task)?
                .ToString();
        }
    }
}