using System.Diagnostics.CodeAnalysis;

namespace CloudStorage.API.Modules.FileSystem.Models
{
    public class MoveObjectRequest
    {
        public string Key { get; set; } = string.Empty;
        public string NewKey { get; set; } = string.Empty;
    }
}
