using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.UndoRedo
{
    public enum CollectionItemChangedType
    {
        Add,
        Remove,
        Move
    }

    public interface ICollectionItem
    {
        void Changed(in CollectionItemChangedInfo info);
    }

    public readonly struct CollectionItemChangedInfo
    {
        public readonly CollectionItemChangedType Type;

        private CollectionItemChangedInfo(in CollectionItemChangedType type)
        {
            Type = type;
        }

        public static readonly CollectionItemChangedInfo Add = new(CollectionItemChangedType.Add);
        public static readonly CollectionItemChangedInfo Remove = new(CollectionItemChangedType.Remove);
        public static readonly CollectionItemChangedInfo Move = new(CollectionItemChangedType.Move);
    }
}
