

using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;


public record TestAggregateRootNameChangedDomainEvent(TestAggregateRoot AggregateRoot) : IDomainEvent;

public record TestPrivateMethodDomainEvent(TestAggregateRoot AggregateRoot) : IDomainEvent;

public record TestEntityNameChangedDomainEvent(TestEntity Entity) : IDomainEvent;

// ========================================
// 用于测试无限递归场景的领域事件
// ========================================

/// <summary>
/// 递归事件A
/// </summary>
public record RecursiveEventA(RecursiveAggregateA AggregateA) : IDomainEvent;

/// <summary>
/// 递归事件B
/// </summary>
public record RecursiveEventB(RecursiveAggregateB AggregateB) : IDomainEvent;

/// <summary>
/// 递归事件C
/// </summary>
public record RecursiveEventC(RecursiveAggregateC AggregateC) : IDomainEvent;

/// <summary>
/// 自引用事件
/// </summary>
public record SelfReferencingEvent(SelfReferencingAggregate Aggregate) : IDomainEvent;
