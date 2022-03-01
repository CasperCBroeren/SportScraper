using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SportScraper
{
    public interface IResultWriter
    {
        void RegisterProducer(string producerName);
        void SaveToOuput(string producerName, UniformGroupLesson item);
        void ProducerEnds(string producerName);

        IReadOnlyList<UniformGroupLesson> All();
    }

    public class XmlResultWriter : IResultWriter
    {
        private int producerCount = 0;
        private Dictionary<string, List<UniformGroupLesson>> items = new Dictionary<string, List<UniformGroupLesson>>();
        private string resultPath;

        public XmlResultWriter(IConfiguration configuration)
        {
            this.resultPath = configuration.GetValue<string>("result");

        }

        public IReadOnlyList<UniformGroupLesson> All() => items.Values.SelectMany(x => x).ToList();
         
        public void ProducerEnds(string producerName)
        {
            if (File.Exists(this.resultPath))
                File.Delete(this.resultPath);
            var toSaveItems = All();
            var x = new XmlSerializer(toSaveItems.GetType());
            using var fileStream = File.Create(this.resultPath);
            x.Serialize(fileStream, toSaveItems);
        }

        public void RegisterProducer(string producerName)
        {
            this.items.Remove(producerName);
            this.items.Add(producerName, new List<UniformGroupLesson>());
            producerCount++;
        }

        public void SaveToOuput(string producerName, UniformGroupLesson item)
        {
            this.items[producerName].Add(item);
        }
         
    }
}
