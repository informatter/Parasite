using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Parasite
{
    public class ParasiteInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Parasite";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("bca90efd-d82a-4179-83bb-604469a4fb24");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
