using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Elsa.Data;
using Elsa.Models;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.YesSql.Documents;
using Elsa.Persistence.YesSql.Extensions;
using Elsa.Persistence.YesSql.Indexes;
using YesSql;
using IIdGenerator = Elsa.Services.IIdGenerator;

namespace Elsa.Persistence.YesSql.Stores
{
    public class YesSqlWorkflowDefinitionStore : YesSqlStore<WorkflowDefinition, WorkflowDefinitionDocument>, IWorkflowDefinitionStore
    {
        public YesSqlWorkflowDefinitionStore(ISession session, IIdGenerator idGenerator, IMapper mapper) : base(session, idGenerator, mapper, CollectionNames.WorkflowDefinitions)
        {
        }

        protected override async Task<WorkflowDefinitionDocument?> FindDocumentAsync(WorkflowDefinition entity, CancellationToken cancellationToken) => await Query<WorkflowDefinitionIndex>(x => x.DefinitionId == entity.Id).FirstOrDefaultAsync();

        protected override IQuery<WorkflowDefinitionDocument> MapSpecification(ISpecification<WorkflowDefinition> specification)
        {
            return specification switch
            {
                EntityIdSpecification<WorkflowDefinition> entityIdSpecification => Query<WorkflowDefinitionIndex>(x => x.DefinitionId == entityIdSpecification.Id),
                VersionOptionsSpecification versionOptionsSpecification => Query<WorkflowDefinitionIndex>().WithVersion(versionOptionsSpecification.VersionOptions),
                WorkflowDefinitionIdSpecification definitionIdSpecification => definitionIdSpecification.VersionOptions == null ? Query<WorkflowDefinitionIndex>(x => x.DefinitionId == definitionIdSpecification.Id) : Query<WorkflowDefinitionIndex>(x => x.DefinitionId == definitionIdSpecification.Id).WithVersion(definitionIdSpecification.VersionOptions),
                _ => AutoMapSpecification<WorkflowDefinitionIndex>(specification)
            };
        }

        protected override IQuery<WorkflowDefinitionDocument> OrderBy(IQuery<WorkflowDefinitionDocument> query, IOrderBy<WorkflowDefinition> orderBy, ISpecification<WorkflowDefinition> specification)
        {
            var expression = orderBy.OrderByExpression.ConvertType<WorkflowDefinition, WorkflowDefinitionDocument>().ConvertType<WorkflowDefinitionDocument, WorkflowDefinitionIndex>();
            var indexedQuery = query.With<WorkflowDefinitionIndex>();
            return orderBy.SortDirection == SortDirection.Ascending ? indexedQuery.OrderBy(expression) : indexedQuery.OrderByDescending(expression);
        }
    }
}