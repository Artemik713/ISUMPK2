using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Models
{
    public class ChatRoomModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
    }
}
