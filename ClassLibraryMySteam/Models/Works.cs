using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.Models
{
    /// <summary>
    /// Одно произведение с информацией о типе
    /// </summary>
    /// <param name="WorkId">id произведения в базе данныхд</param>
    /// <param name="Title">название произведения</param>
    /// <param name="TypeName">имя типа произведения</param>
    /// <param name="Year">год выпуска</param>
    /// <param name="Rating">рейтинг (Decimal число)</param>
    /// <param name="CoverPath">путь к иконке произведения (возможно потом реализую)</param>
    public record WorkItem(
        int WorkId,
        string Title,
        string TypeName,
        int? Year,
        double? Rating,
        string? CoverPath
    );
}
