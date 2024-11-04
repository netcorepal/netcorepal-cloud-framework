using NetCorePal.Extensions.Dto;
namespace NetCorePal.Extensions.Dto.Tests
{
    public class PageQueryableExtensionsTests
    {
        [Fact]
        public void ToPagedDataTests()
        {
            var allStrings = new string[] { "a", "b", "c" }.AsQueryable();

            var pagedData = allStrings.ToPagedData(1, 2, true);
            Assert.Equal(2, pagedData.Items.Count());
            Assert.Equal(3, pagedData.Total);
            Assert.Equal(1, pagedData.Index);

            pagedData = allStrings.ToPagedData(2, 2, true);
            Assert.Equal(1, pagedData.Items.Count());
            Assert.Equal(3, pagedData.Total);
            Assert.Equal(2, pagedData.Index);

            pagedData = allStrings.ToPagedData(3, 2, true);
            Assert.Equal(0, pagedData.Items.Count());
            Assert.Equal(3, pagedData.Total);
            Assert.Equal(3, pagedData.Index);


            pagedData = allStrings.ToPagedData(1, 2, false);

            Assert.Equal(2, pagedData.Items.Count());
            Assert.Equal(0, pagedData.Total);
            Assert.Equal(1, pagedData.Index);

            pagedData = allStrings.ToPagedData(2, 2, false);
            Assert.Equal(1, pagedData.Items.Count());
            Assert.Equal(0, pagedData.Total);
            Assert.Equal(2, pagedData.Index);


            Assert.Throws<ArgumentOutOfRangeException>(() => allStrings.ToPagedData(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => allStrings.ToPagedData(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => allStrings.ToPagedData(1, 0));

        }
    }
}