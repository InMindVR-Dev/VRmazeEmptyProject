using HumanVirtualMaze.Interfaces;
using HumanVirtualMaze.Internal;
using HumanVirtualMaze.Model;
using HumanVirtualMaze.Model.Protocols;
using HumanVirtualMaze.Model.Recording;
using HumanVirtualMaze.Model.Trackers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmptyVRmazeProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadPlugins();

            Protocol loaded = (Protocol) StaticFunctions.ReadIObjectBySerialization(typeof(Protocol), args[0]);
            
            //Depuis le protocole, on rempli une liste d'accession direct a l'ensemble (presque tous) des entités du protocole
            loaded.PopulateObjectIdList();

            //On va chercher un type spécifique dans la liste de tous les objets du protocole
            foreach (var item in loaded.GetObjectIDList()) // On parcours la liste
            {                

                //Si le type de l'entité courrante est un DataRecord, alors on va travailler dessus
                if (typeof(DataRecord).IsAssignableFrom(item.GetType()))
                {
                    //Conversion pour être plus rapide dans l'ecriture des prochaines lignes
                    DataRecord dr = (DataRecord)item;
                    
                    //On recherche l'entité qui contient le DataRecord. N'importe quelle entité qui en contient un implémente l'interface IRecordable
                    //Donc on recherche le premier parent IRecordable dans les parents du Datarecord (ici, on remonte dans la hierarchie)
                    IRecordable parentEntity = (IRecordable) dr.HasSpecifiedTypeParents(typeof(IRecordable));

                    //Si le parent n'est pas null (ca ne doit normalement jamais arriver)
                    if(parentEntity != null)
                    {
                        //On ecrit son nom et son type
                        Console.WriteLine(parentEntity.Name + " " + parentEntity.GetType().FullName);
                    }
                    
                    //Et on écrit quelques données du Datarecord (nombre de trackers et quelques timescodes)
                    Console.Write ("\t" + dr.Name + " Total Tracker Count : " +  dr.Out.Count);
                    Console.Write(" Tracker Type : " + dr.Out.TrackerType);
                    Console.WriteLine(" Timecodes : [" + dr.GetOutFirstTimeCode().Round(3).ToString(CultureInfo.InvariantCulture) + "," + dr.GetOutLastTimeCode().Round(3).ToString(CultureInfo.InvariantCulture)+"]");
                }
            }

            Console.WriteLine("Done !");

        }

        //Fonction de lecture des Plugins dans le dossier plugins du projet
        public static void ReadPlugins()
        {
            Console.WriteLine("Reading Plugins");

            string[] dllFileNames = null;
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Plugins/"))
            {
                dllFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Plugins/", "*.dll");
            }
            else
            {
                StaticFunctions.LogToConsole(" " + "Erreur de path");
            }

            string[] customDllFilenames = null;
            if (Directory.Exists(StaticFunctions.GetVRmazePersonnalFolder() + "/CustomPlugins/"))
            {
                customDllFilenames = Directory.GetFiles(StaticFunctions.GetVRmazePersonnalFolder() + "/CustomPlugins/", "*.dll");
            }
            else
            {
                StaticFunctions.LogToConsole("Erreur de custom path");
            }

            ICollection<Assembly> assemblies = new List<Assembly>();

            try
            {
                if (dllFileNames != null)
                {
                    foreach (string dllFile in dllFileNames)
                    {

                        StaticFunctions.LogToConsole(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + "Try to load " + dllFile);
                        //if (!dllFile.ToLower().Contains("view"))
                        //{
                        AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                        Assembly assembly = Assembly.Load(an);
                        if (assembly == null)
                        {
                            Console.WriteLine("Assembly cannot be read");
                        }
                        assemblies.Add(assembly);
                        //}
                    }
                }

                if (customDllFilenames != null)
                {
                    foreach (string dllFile in customDllFilenames)
                    {

                        StaticFunctions.LogToConsole(System.Reflection.MethodBase.GetCurrentMethod().Name + " " + "Try to load " + dllFile);
                        AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                        Assembly assembly = Assembly.Load(an);
                        assemblies.Add(assembly);
                    }
                }
            }

            catch (System.Exception ex)
            {
                StaticFunctions.LogToConsole("Error loading plugins ! " + ex.Message);
            }

            Console.WriteLine("Reading Plugins done !");

        }

    }
}
