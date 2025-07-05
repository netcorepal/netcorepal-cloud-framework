using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 用户ID强类型ID
/// </summary>
public partial record UserId : IGuidStronglyTypedId;

/// <summary>
/// 用户聚合根
/// </summary>
public class User : Entity<UserId>, IAggregateRoot
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public bool IsActive { get; private set; }

    protected User()
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public User(UserId id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
        IsActive = true;
        
        AddDomainEvent(new UserCreatedDomainEvent(this));
    }

    /// <summary>
    /// 激活用户
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new UserActivatedDomainEvent(this));
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedDomainEvent(this));
    }

    /// <summary>
    /// 完成用户注册流程
    /// </summary>
    public void CompleteRegistration()
    {
        // 发出用户注册完成领域事件
        AddDomainEvent(new UserRegisteredDomainEvent(this, DateTime.UtcNow));
    }
}
