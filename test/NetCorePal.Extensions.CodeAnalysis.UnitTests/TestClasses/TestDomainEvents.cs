

using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public record TestAggregateRootNameChangedDomainEvent(TestAggregateRoot AggregateRoot) : IDomainEvent;

public record TestPrivateMethodDomainEvent(TestAggregateRoot AggregateRoot) : IDomainEvent;

public record TestEntityNameChangedDomainEvent(TestEntity Entity) : IDomainEvent;
