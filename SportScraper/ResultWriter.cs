using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SportScraper
{
    public interface IResultWriter
    {
        void RegisterProducer();
        void SaveToOuput(UniformGroupLesson item);
        void ProducerEnds();

        IReadOnlyList<UniformGroupLesson> All();
    }

    public class XmlResultWriter : IResultWriter
    {
        private int producerCount = 0;
        private List<UniformGroupLesson> items = new List<UniformGroupLesson>();
        private string resultPath;

        public XmlResultWriter(IConfiguration configuration)
        {
            this.resultPath = configuration.GetValue<string>("result");

        }

        public IReadOnlyList<UniformGroupLesson> All() => items;

        public void ProducerEnds()
        {
            producerCount--;
            if (producerCount == 0)
            {
                if (File.Exists(this.resultPath))
                    File.Delete(this.resultPath);

                var x = new XmlSerializer(items.GetType());
                using var fileStream = File.Create(this.resultPath);
                x.Serialize(fileStream, items);
            }
        }

        public void RegisterProducer()
        {
            producerCount++;
        }

        public void SaveToOuput(UniformGroupLesson item)
        {
            this.items.Add(item);
        }
    }
}
