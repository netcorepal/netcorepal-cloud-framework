using DotNetCore.CAP.Internal;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public static class StatusNameHelper
{
    public static string GetRealStatusName(string statusName)
    {
        //输入的statusName可能大小写不一致，进行转换，转换为StatusName枚举，再转换为字符串返回

        if (Enum.TryParse<StatusName>(statusName, ignoreCase: true, out var status))
        {
            return status.ToString();
        }

        return string.Empty;
    }
}