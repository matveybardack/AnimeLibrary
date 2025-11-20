using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.Models
{
    /// <summary>
    /// Представление одного тега
    /// </summary>
    /// <param name="TagId">id тега в бд</param>
    /// <param name="Name">название тега</param>
    public record TagItem(
        int TagId,
        string Name
    );
}
