
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

public partial record TestAggregateRootId : IGuidStronglyTypedId;


public class TestAggregateRoot : Entity<TestAggregateRootId>, IAggregateRoot
{
    protected TestAggregateRoot()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public TestEntity TestEntity { get; set; } = new TestEntity(1, "测试实体");

    public ICollection<TestEntity2> TestEntities { get; set; } = new List<TestEntity2>();

    public TestAggregateRoot(TestAggregateRootId id)
    {
        Id = id;
    }

    /// <summary>
    /// 静态方法
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static TestAggregateRoot Create(TestAggregateRootId id)
    {
        return new TestAggregateRoot()
        {
            Id = id,
            Name = "默认名称"
        };
    }

    public void ChangeName(string name)
    {
        Name = name;
        // 不使用this关键字，直接调用AddDomainEvent
        AddDomainEvent(new TestAggregateRootNameChangedDomainEvent(this));
    }


    /// <summary>
    /// 私有方法
    /// </summary>
    private void PrivateMethod()
    {
        // 调用子实体方法
        this.TestEntity.ChangeTestEntityName("新名称");
        // 使用this关键字，调用AddDomainEvent
        this.AddDomainEvent(new TestPrivateMethodDomainEvent(this));
    }
}


public class TestEntity : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public TestEntity(int id, string name)
    {
        Id = id;
        Name = name;
    }

    protected TestEntity()
    {
    }

    public void ChangeTestEntityName(string name)
    {
        Name = name;
        // 直接调用AddDomainEvent
        AddDomainEvent(new TestEntityNameChangedDomainEvent(this));
    }
}


public class TestEntity2 : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public TestEntity2(int id, string name)
    {
        Id = id;
        Name = name;
    }

    protected TestEntity2()
    {
    }
}

// ========================================
// 用于测试无限递归场景的实体类
// ========================================

/// <summary>
/// 递归聚合根A - 包含B类型的子实体
/// </summary>
public partial record RecursiveAggregateAId : IGuidStronglyTypedId;

public class RecursiveAggregateA : Entity<RecursiveAggregateAId>, IAggregateRoot
{
    protected RecursiveAggregateA()
    {
    }

    public string Name { get; private set; } = string.Empty;
    
    // 包含递归聚合根B作为子实体
    public ICollection<RecursiveAggregateB> SubEntitiesB { get; set; } = new List<RecursiveAggregateB>();
    
    // 包含递归聚合根C作为子实体，形成A->B->C->A的循环
    public ICollection<RecursiveAggregateC> SubEntitiesC { get; set; } = new List<RecursiveAggregateC>();

    public RecursiveAggregateA(RecursiveAggregateAId id)
    {
        Id = id;
    }

    public void ProcessA()
    {
        AddDomainEvent(new RecursiveEventA(this));
    }
}

/// <summary>
/// 递归聚合根B - 包含A类型的子实体，形成循环引用
/// </summary>
public partial record RecursiveAggregateBId : IGuidStronglyTypedId;

public class RecursiveAggregateB : Entity<RecursiveAggregateBId>, IAggregateRoot
{
    protected RecursiveAggregateB()
    {
    }

    public string Name { get; private set; } = string.Empty;
    
    // 包含递归聚合根A作为子实体，形成循环引用
    public ICollection<RecursiveAggregateA> SubEntitiesA { get; set; } = new List<RecursiveAggregateA>();

    public RecursiveAggregateB(RecursiveAggregateBId id)
    {
        Id = id;
    }

    public void ProcessB()
    {
        AddDomainEvent(new RecursiveEventB(this));
    }
}

/// <summary>
/// 自引用聚合根 - 包含自己类型的子实体
/// </summary>
public partial record SelfReferencingAggregateId : IGuidStronglyTypedId;

public class SelfReferencingAggregate : Entity<SelfReferencingAggregateId>, IAggregateRoot
{
    protected SelfReferencingAggregate()
    {
    }

    public string Name { get; private set; } = string.Empty;
    
    // 自引用：包含自己类型的子实体
    public ICollection<SelfReferencingAggregate> Children { get; set; } = new List<SelfReferencingAggregate>();
    
    public SelfReferencingAggregate? Parent { get; set; }

    public SelfReferencingAggregate(SelfReferencingAggregateId id)
    {
        Id = id;
    }

    public void ProcessSelf()
    {
        AddDomainEvent(new SelfReferencingEvent(this));
        
        // 可能调用子实体的方法，这会形成潜在的递归
        foreach (var child in Children)
        {
            child.ProcessSelf();
        }
    }
}

/// <summary>
/// 复杂循环引用聚合根C - 三层循环 A->B->C->A
/// </summary>
public partial record RecursiveAggregateCId : IGuidStronglyTypedId;

public class RecursiveAggregateC : Entity<RecursiveAggregateCId>, IAggregateRoot
{
    protected RecursiveAggregateC()
    {
    }

    public string Name { get; private set; } = string.Empty;
    
    // 包含递归聚合根A作为子实体，完成A->B->C->A的循环
    public ICollection<RecursiveAggregateA> SubEntitiesA { get; set; } = new List<RecursiveAggregateA>();

    public RecursiveAggregateC(RecursiveAggregateCId id)
    {
        Id = id;
    }

    public void ProcessC()
    {
        AddDomainEvent(new RecursiveEventC(this));
    }
}