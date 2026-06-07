namespace SportMap.Application.DTOs.Common;

public class PagedResponse<T>
{
    // البيانات في الصفحة دي
    public List<T> Data { get; set; } = new();

    // رقم الصفحة الحالية
    public int CurrentPage { get; set; }

    // عدد العناصر في كل صفحة
    public int PageSize { get; set; }

    // إجمالي عدد العناصر كلها
    public int TotalCount { get; set; }

    // إجمالي عدد الصفحات
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // في صفحة قبلها؟
    public bool HasPrevious => CurrentPage > 1;

    // في صفحة بعدها؟
    public bool HasNext => CurrentPage < TotalPages;
}