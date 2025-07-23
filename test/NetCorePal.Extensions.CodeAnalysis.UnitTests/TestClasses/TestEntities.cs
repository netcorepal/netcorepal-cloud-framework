
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
        this.TestEntity.ChangeTestEntity2Name("新名称");
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

    public void ChangeTestEntity2Name(string name)
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