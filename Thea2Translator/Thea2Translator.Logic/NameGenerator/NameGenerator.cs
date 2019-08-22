using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameGenerator
    {
        private readonly string FullPath;
        public IList<NameGeneratorElem> NameGeneratorElems { get; private set; }

        public NameGenerator(DirectoryType directoryType = DirectoryType.Cache)
        {
            FullPath = FileHelper.GetLocalFilePatch(directoryType, FilesType.NamesGenerator);
            ResetElems();
        }

        public NameGeneratorElem GetElemByCollectionName(string collectionName)
        {
            foreach (var elem in NameGeneratorElems)
            {
                if (elem.Collection == collectionName)
                    return elem;
            }

            return null;
        }

        private void ResetElems()
        {
            NameGeneratorElems = new List<NameGeneratorElem>();
        }

        public void LoadFromFile()
        {
            ResetElems();

            if (!FileHelper.FileExists(FullPath))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(FullPath);
            var elements = doc.DocumentElement.GetElementsByTagName("Element");
            foreach (XmlNode element in elements)
            {
                var elem = new NameGeneratorElem(element);
                AddElem(elem);
            }
        }

        protected void AddElem(NameGeneratorElem elem)
        {
            NameGeneratorElems.Add(elem);
        }

        public static void MergeCache()
        {
            //Narazie nie merguje, chyba nie ma takiego powodu, biore jak jest z olda
            var original = new NameGenerator(DirectoryType.CacheOld);
            if (!FileHelper.FileExists(original.FullPath)) original = new NameGenerator(DirectoryType.Original);
            FileHelper.CopyFile(original.FullPath, DirectoryType.Cache);
        }
    }
}
