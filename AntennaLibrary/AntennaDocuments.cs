using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntennaLibrary
{
    public class AntennaDocumentsRoot
    {
        public string Name { get; set; }

        public ObservableCollection<AntennaCategory> Categories { get; set; } = new ObservableCollection<AntennaCategory>();

        public void LoadFromFile(string filename)
        {
            XDocument xDoc = XDocument.Load(filename);
            if (xDoc.Root != null)
            {
                foreach (var category in xDoc.Root.Descendants("Category"))
                {
                    var antennaCategory = new AntennaCategory();
                    antennaCategory.Name = category.Attribute("Name").Value;

                    foreach (var subCategory in category.Descendants("SubCategory"))
                    {
                        var antennaSubCategory = new AntennaSubCategory();
                        antennaSubCategory.Name = subCategory.Attribute("Name").Value;

                        foreach (var antenna in subCategory.Descendants("Antenna"))
                        {
                            var antennaDocument = new AntennaDocument();
                            antennaDocument.Name = antenna.Attribute("Name").Value;
                            antennaDocument.Document = antenna.Element("Document").Value;
                            antennaSubCategory.AntennaDocuments.Add(antennaDocument);
                        }

                        antennaCategory.SubCategories.Add(antennaSubCategory);
                    }

                    Categories.Add(antennaCategory);
                }
            }
        }
    }

    public class AntennaCategory
    {
        public string Name { get; set; }

        public ObservableCollection<AntennaSubCategory> SubCategories { get; set; } = new ObservableCollection<AntennaSubCategory>();
    }

    public class AntennaSubCategory
    {
        public string Name { get; set; }

        public ObservableCollection<AntennaDocument> AntennaDocuments { get; set; } = new ObservableCollection<AntennaDocument>();
    }

    public class AntennaDocument
    {
        public string Name { get; set; }

        public string Document { get; set; }
    }
}
