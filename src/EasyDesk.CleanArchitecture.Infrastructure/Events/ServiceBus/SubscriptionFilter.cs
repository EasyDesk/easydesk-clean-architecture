using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public abstract class SubscriptionFilter
    {
        protected abstract string SqlExpression();

        public RuleFilter GetRuleFilter() => new SqlRuleFilter(SqlExpression());
    }

    public class EventTypeFilter : SubscriptionFilter
    {
        private readonly IEnumerable<string> _types;

        public EventTypeFilter(IEnumerable<string> types)
        {
            _types = types;
        }

        protected override string SqlExpression() => $"{PropertyNames.EventType} IN {GetCommaSeparatedTypeList()}";

        private string GetCommaSeparatedTypeList() => _types.ToTupleString(t => $"'{t}'");
    }

    public class OrganizationScopedFilter : SubscriptionFilter
    {
        private readonly Guid _organizationId;

        public OrganizationScopedFilter(Guid organizationId)
        {
            _organizationId = organizationId;
        }

        protected override string SqlExpression() => $"{PropertyNames.OrganizationId} = '{_organizationId}'";
    }

    public abstract class BooleanOperationFilter : SubscriptionFilter
    {
        private readonly IEnumerable<SubscriptionFilter> _terms;
        private readonly string _operatorName;

        public BooleanOperationFilter(IEnumerable<SubscriptionFilter> terms, string operatorName)
        {
            _terms = terms;
            _operatorName = operatorName;
        }

        protected override string SqlExpression() => _terms
            .Select(t => $"({t})")
            .ConcatStrings($" {_operatorName} ");
    }

    public class OrFilter : BooleanOperationFilter
    {
        public OrFilter(IEnumerable<SubscriptionFilter> terms) : base(terms, "OR") { }
    }

    public class AndFilter : BooleanOperationFilter
    {
        public AndFilter(IEnumerable<SubscriptionFilter> terms) : base(terms, "AND") { }
    }
}
