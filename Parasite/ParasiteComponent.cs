using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper;

using Newtonsoft.Json;
// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Parasite
{
    /// <summary>
    /// This plugin was developed by Dr. Roland Hudson and Nicholas Rawitscher
    /// </summary>
    public class ParasiteComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ParasiteComponent()
          : base("Parasite", "Parasite",
              "get some data",
              "DRL", "DataMining")
        {
        }




        public string client = "";
        public string secret = "";
        public int nResults = 2; //2
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
            //pManager.AddTextParameter("4sqPath", "4sqp", "Path to query online", GH_ParamAccess.item, "https://api.foursquare.com/v2/venues/search?client_id=1MTHQO5AXBTCEI5S24JTCBBNPVWNVP13HI03A1FVMRHDF2P0&client_secret=4MQLNPFUNM2GFNOFAP3JDXQFWMHN3DINMLVKRBJC3HT4XWFY&v=20150929&ll=4.6,-74");
            pManager.AddTextParameter("Client id", "id", "your foresquare client id", GH_ParamAccess.item, " NFIUJRUBJV0BILYSGMLI5NS4OXAN21T22A21DQCQUDBMHHFO"); // original //1MTHQO5AXBTCEI5S24JTCBBNPVWNVP13HI03A1FVMRHDF2P0  // new  NFIUJRUBJV0BILYSGMLI5NS4OXAN21T22A21DQCQUDBMHHFO
            pManager.AddTextParameter("Client secret", "secret", "your foresquare client secret", GH_ParamAccess.item, "2VTDNJA01HCAJBWZYQDPFKMNLVYAKEQTSV3ZBANBPUZXUN5F"); // original// 4MQLNPFUNM2GFNOFAP3JDXQFWMHN3DINMLVKRBJC3HT4XWFY  // new 2VTDNJA01HCAJBWZYQDPFKMNLVYAKEQTSV3ZBANBPUZXUN5F
            pManager.AddNumberParameter("northern most latitude", "north latitude", "northern most latitude of search box", GH_ParamAccess.item, 51.5274);
            pManager.AddNumberParameter("southern most latitude", "south latitude", "southern most latitude of search box", GH_ParamAccess.item, 51.5030);
            pManager.AddNumberParameter("eastern most longitude", "east longitude", "eastern most longitude of search box", GH_ParamAccess.item, -0.0710);
            pManager.AddNumberParameter("western most longitude", "west longitude", "western most longitude of search box", GH_ParamAccess.item, -0.1146);
            pManager.AddIntegerParameter("granularity of search", "granularity", "forsquare only permits max 5000 requests per hour, enter 2-10. 3=3X3 grid within bounds", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("maximum number of results", "max results", "maximum number of results per cell granularity", GH_ParamAccess.item, 0);

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddTextParameter("latitudes", "lat", "latitudes from source", GH_ParamAccess.tree);
            pManager.AddTextParameter("longitudes", "lon", "longitudes from source", GH_ParamAccess.tree);
            pManager.AddTextParameter("venue name", "venue name", "name of each venue", GH_ParamAccess.tree);
            pManager.AddTextParameter("venue category", "venue category", "category of each venue", GH_ParamAccess.tree);
            pManager.AddTextParameter("queryURL", "url", "url used to query api foursquare", GH_ParamAccess.item);
            pManager.AddTextParameter("checkins", "checkins", "total checkins from each venue", GH_ParamAccess.tree);
            pManager.AddTextParameter("usercount", "usercount", "total users from each venue", GH_ParamAccess.tree);
            pManager.AddNumberParameter("tipcount", "tipcount", "total tips from each venue", GH_ParamAccess.tree);
            //pManager.AddTextParameter("herenow", "herenow", "total people that are in each individual at the current query ", GH_ParamAccess.tree);


            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.

            string urlfile = "";
            double northlat = 54.5; //54.5
            double southlat = 54.5; //54.5
            double eastlong = 0.0;
            double westlong = 0.0;
            int gran = 5; //5


            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref client)) return;
            if (!DA.GetData(1, ref secret)) return;
            if (!DA.GetData(2, ref northlat)) return;
            if (!DA.GetData(3, ref southlat)) return;
            if (!DA.GetData(4, ref eastlong)) return;
            if (!DA.GetData(5, ref westlong)) return;
            if (!DA.GetData(6, ref gran)) return;
            if (!DA.GetData(7, ref nResults)) return;


            //DA.GetData(0, ref client);
            // DA.GetData(1, ref secret);
            // DA.GetData(2, ref northlat);
            // DA.GetData(3, ref southlat);
            // DA.GetData(4, ref eastlong);
            // DA.GetData(5, ref westlong);
            //  DA.GetData(6, ref gran);
            // DA.GetData(7, ref nResults);


            // We should now validate the data and warn the user if invalid data is supplied.

            GH_Structure<GH_Number> latitudes = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> longitudes = new GH_Structure<GH_Number>();
            GH_Structure<GH_String> venuename = new GH_Structure<GH_String>();
            //GH_Structure<GH_String> categoryshortname = new GH_Structure<GH_String>();
            GH_Structure<GH_Integer> checks = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Integer> count = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Integer> tips = new GH_Structure<GH_Integer>();
            //GH_Structure<GH_Integer> hereno = new GH_Structure<GH_Integer>();
            double celllat = Math.Abs(northlat - southlat) / gran;
            double celllng = Math.Abs(eastlong - westlong) / gran;
            double n = 0; double s = 0; double e = 0; double w = 0;

            for (int i = 0; i < gran; i++)
            {
                for (int j = 0; j < gran; j++)
                {
                    w = westlong + i * celllng;
                    e = westlong + (i + 1) * celllng;
                    s = southlat + j * celllat;
                    n = southlat + (j + 1) * celllat;
                    urlfile = buildurl(n, s, e, w);
                    readfromwebJsonLatLong(urlfile, ref latitudes, ref longitudes, ref venuename, ref checks, ref count, ref tips);

                }
            }




            // Finally assign the the output parameters.

            DA.SetDataTree(0, latitudes);
            DA.SetDataTree(1, longitudes);
            DA.SetDataTree(2, venuename);
            //DA.SetDataTree(3, categoryshortname);
            DA.SetData(4, urlfile);
            DA.SetDataTree(5, checks);
            DA.SetDataTree(6, count);
            DA.SetDataTree(7, tips);
            //DA.SetDataTree(8, hereno);
        }

        private string buildurl(double n, double s, double e, double w)
        {
            
            ///////https://api.foursquare.com/v2/venues/search?
            string urlfile = "https://api.foursquare.com/v2/venues/search?" +
             "client_id=" + client +
           "&client_secret=" + secret +
           "&v=20180323" +//version  "&v=20150929"    20180323
           "&intent=browse" +
           "&limit=" + nResults.ToString() +
           "&sw=" + s.ToString() + "," + w.ToString() +
           "&ne=" + n.ToString() + "," + e.ToString();


            return urlfile;

        }

        //private string readfromlocal(string filepath)
        //{
        //    StreamReader reader = new StreamReader(filepath);
        //    string all = reader.ReadToEnd();
        //    return all;
        //}



        //private string readfromweb(string url)

        //{
        //    WebClient client = new WebClient();

        //    // Download string.

        //    string value = client.DownloadString(url);
        //    return value;

        //}



        private void readfromwebJsonLatLong(string url, ref GH_Structure<GH_Number> lats, ref GH_Structure<GH_Number> longs, ref GH_Structure<GH_String> ven,
            ref GH_Structure<GH_Integer> checks, ref GH_Structure<GH_Integer> count, ref GH_Structure<GH_Integer> tips)

        {
            WebClient client = new WebClient();

            // Download string.
            string value = client.DownloadString(url);
            JsonTextReader reader = new JsonTextReader(new StringReader(value));
            string venuename = "";
            string categoryshortname = "";
            string venuelat = "";
            string venuelng = "";
            string checkins = "";
            string usercount = "";
            string tipcount = "";
            //string herenow = "";

            while (reader.Read())

            {
                if (reader.Value != null)
                {

                    string val = reader.Value.ToString();
                    if (val == "name")
                    {
                        reader.Read();
                        venuename = reader.Value.ToString();

                    }

                    if (val == "shortName")
                    {

                        reader.Read();
                        categoryshortname = reader.Value.ToString();

                    }



                    if (val == "lat")
                    {
                        reader.Read();
                        venuelat = reader.Value.ToString();
                    }

                    if (val == "lng")
                    {
                        reader.Read();
                        venuelng = reader.Value.ToString();
                    }


                    if (val == "checkinsCount")
                    {
                        reader.Read();
                        checkins = reader.Value.ToString();

                    }

                    if (val == "usersCount")
                    {
                        reader.Read();
                        usercount = reader.Value.ToString();
                    }

                    if (val == "tipCount")
                    {
                        reader.Read();
                        tipcount = reader.Value.ToString();

                    }

                    //if (val == "hereNow")
                    //{
                    //    reader.Read();
                    //    herenow= reader.Value.ToString();
                    //}


                    if (venuename != "" && venuelat != "" && venuelng != "" && categoryshortname != "" && checkins != "" && usercount != "" && tipcount != "") //&& herenow != "")
                    {
                        //we have name and postition so fill the structures
                        ven.Append(new GH_String(venuename));
                        lats.Append(new GH_Number(float.Parse(venuelat, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                        longs.Append(new GH_Number(float.Parse(venuelng, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                        checks.Append(new GH_Integer(int.Parse(checkins, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                        count.Append(new GH_Integer(int.Parse(usercount, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                        tips.Append(new GH_Integer(int.Parse(tipcount, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));
                        //hereno.Append(new GH_Integer(int.Parse(herenow, System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));

                        //and reset value holders
                        venuename = "";
                        categoryshortname = "";
                        venuelat = "";
                        venuelng = "";
                        checkins = "";
                        usercount = "";
                        tipcount = "";
                        //herenow = "";
                    }



                }//end of while loop



            }
        }

        //private GH_Structure<GH_String> readfromwebJson(string url)
        //{
        //    WebClient client = new WebClient();

        //    // Download string.
        //    string value = client.DownloadString(url);
        //    JsonTextReader reader = new JsonTextReader(new StringReader(value));
        //    DataTree<string> dataset = new DataTree<string>();
        //    GH_Structure<GH_String> dataout = new GH_Structure<GH_String>();
        //    while (reader.Read())
        //    {
        //        if (reader.Value != null)
        //        {
        //            dataset.Add(reader.TokenType.ToString());
        //            dataset.Add(reader.Value.ToString());

        //            //GH_String token = new GH_String(reader..ToString());
        //            string val = reader.Value.ToString();
        //            if (val == "lat" || val == "lng")
        //            {
        //                dataout.Append(new GH_String(val));
        //                reader.Read();
        //                val = reader.Value.ToString();
        //                dataout.Append(new GH_String(val));
        //            }
        //            //dataout.Append(token);

        //        }

        //    }
        //    return dataout;
        //}





        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{6846ce93-07c2-4ce4-9f4e-7e8732ad46a5}"); }
        }


    }
}
