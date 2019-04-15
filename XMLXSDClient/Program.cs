using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Schemas;

namespace XMLXSDClient
{
    internal class Program
    {
        private static void WriteXml(StreamWriter stream)
        {
            var pedido = new Pedido {Metodo = "getHistorico"};
            var matriculaArg = new ArgumentoType {Valor = "2016780485", Nome = "matricula"};
            pedido.Argumentos = new[] {matriculaArg};

            var xmlSerializer = new XmlSerializer(typeof(Pedido));
            xmlSerializer.Serialize(stream, pedido);
        }

        private static HistoricoEscolar ReadXml(StreamReader reader)
        {
            using (var schemaReader = XmlReader.Create("historico.xsd"))
            {
                var schemas = new XmlSchemaSet();
                schemas.Add(XmlSchema.Read(schemaReader, null));
                
                var settings = new XmlReaderSettings {Schemas = schemas};

                using (var historicoReader = XmlReader.Create(reader, settings))
                {
                    var xmlSerializer = new XmlSerializer(typeof(HistoricoEscolar));
                    return (HistoricoEscolar) xmlSerializer.Deserialize(historicoReader);
                }
            }
        }

        private static void PrintHistorico(HistoricoEscolar historicoEscolar)
        {
           Console.WriteLine("Aluno: {0}", historicoEscolar.Aluno.Nome); 
           Console.WriteLine("Matrícula: {0}", historicoEscolar.Aluno.Matricula);
           Console.WriteLine("Curso: {0}", historicoEscolar.Aluno.Curso);
           
           foreach (var periodo in historicoEscolar.Periodos)
           {
               Console.WriteLine("Período: {0}", periodo.Nome);

               foreach (var materia in periodo.Materias)
               {
                   Console.WriteLine("Materia: {0}", materia.Nome);
                   Console.WriteLine("Codigo: {0}", materia.Codigo);
                   Console.WriteLine("Resultado: {0}", materia.Resultado);
                   Console.WriteLine("Situacao: {0}", materia.Situacao);
               }
           }
        }
        
        public static void Main(string[] args)
        {
            using (var tcpClient = new TcpClient("localhost", 5000))
            using (var stream = tcpClient.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 64, true))
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 64, true))
            {
                WriteXml(writer);
                tcpClient.Client.Shutdown(SocketShutdown.Send);

                var historico = ReadXml(reader);
                PrintHistorico(historico);
            }
        }
    }
}