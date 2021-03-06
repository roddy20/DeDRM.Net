﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ionic.Zip;

namespace DeDRM.Library.Base
{
    public abstract class EpubHandler : IRemoveDrm
    {
        //Global Class Properties. 
        public String InputZipFilePath { get; set; }
        public String OutputZipFilePath { get; set; }
        //NameSpaces
        protected static XNamespace adeptns = "http://ns.adobe.com/adept";
        protected static XNamespace applens = "http://itunes.apple.com/ns/epub";
        protected static XNamespace encryptionns = "http://www.w3.org/2001/04/xmlenc#";


        public static Boolean DetectDrm(ZipFile inputZip, out Constants.DRMType drmType)
        {
            drmType = Constants.DRMType.None;
            //inputZip["META-INF/encryption.xml"];
            try
            {
                // Adobe Adept DRM has "META-INF/rights.xml", containing <operatorURL>.
                // B&N have an <operatorURL> with "barnesandnoble.com" somewhere inside.
                // Adobe uses a variety of other URLs.
                ZipEntry rightsXml = inputZip["META-INF/rights.xml"];
                if (rightsXml != null)
                {
                    XElement xRoot = XElement.Load(rightsXml.OpenReader());
                    

                    var adeptDrm = xRoot.Descendants(adeptns + "operatorURL");
                    if (adeptDrm.Count() > 0)
                    {
                        String url = adeptDrm.Last().Value;
                        if (url.ToLower().Contains("barnesandnoble"))
                        {
                            drmType =  Constants.DRMType.BarnesAndNoble;
                            return true;
                        }
                        else
                        {

                            drmType = Constants.DRMType.Adobe;

                            return true;
                        }
                    }


                    XNamespace ns = (xRoot.Attribute("xmlns") != null) ? xRoot.Attribute("xmlns").Value : XNamespace.None;

                    var koboDRM = xRoot.Name;
                    if (koboDRM.ToString().ToLower() == "kdrm")
                    {
                        drmType = Constants.DRMType.Kobo;
                        return true;
                    }
                    

                }
                ZipEntry sinfXml = inputZip["META-INF/sinf.xml"];
                // Apple Fairplay DRM has "META-INF/sinf.xml" containing <fairplay:sinf>.
                if (sinfXml != null)
                {
                    XElement xRoot = XElement.Load(sinfXml.InputStream);
                    //XNamespace ns = (xRoot.Attribute("xmlns") != null) ? xRoot.Attribute("xmlns").Value : XNamespace.None;
                    
                    var fairplayDRM = xRoot.Descendants(applens + "sinf");
                    if (fairplayDRM.Count() > 0)
                    {
                        drmType = Constants.DRMType.Apple;
                        return true;
                    }
                }

                return false;

            }

            catch (Exception e)
            {
                return false;
            }
        }

        public abstract void RemoveDrm(String inputZipFilePath, string outputZipFilePath);
    }
}
