﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using pwApi.Readers;
using pwApi.StructuresElement;

namespace editor
{
    class Helper
    {
        public static ElementReader _elReader;
        public static Image _img;
        public static List<string> _surfaces;
        public static Dictionary<string, Image> _cropped;

        public static Image GetImage(int id)
        {
            try
            {
                var path = _elReader.GetIcon(Convert.ToInt32(id));
                path = Path.GetFileName(path.Replace("\0", ""));
                if (_cropped.ContainsKey(path))
                    return _cropped[path];
            }
            catch (Exception)
            {
            }
            return _cropped.ElementAt(0).Value;

        }
        public static void CropImages()
        {
            _cropped = new Dictionary<string, Image>();
            foreach (var ll in _surfaces)
            {
                _cropped.Add(ll,Graphic.GetImage(_img,ll));
            }
            Graphic.bmpImage.Dispose();
            Graphic.bmpImage = null;
        }
        public static List<string> DropSearcher(Item it)
        {
            var ls = new List<string>();
            foreach (var line in _elReader.GetListById(39))
            {
                var id = it.GetByKey("ID");
                for (int i = 189; i < 252; i++)
                {
                    var zz = (string)line.Values[i, 0];
                    if (zz.Contains("drop_matters_") && zz.Contains("id"))
                    {
                        if (id == (int)line.Values[i, 1])
                            ls.Add(String.Format("List : {0} ID : {1} Name : {2} Шанс : {3}", 39, line.GetByKey("ID"),
                                ((string)line.GetByKey("Name")).Replace("\0",""), line.Values[++i, 1]));
                    }
                }
            }
            return ls;
        }

        public static List<string> CraftSearch(Item it)
        {
            var ls = new List<string>();
            int id = Convert.ToInt32(it.GetByKey("ID"));
            foreach (var item in _elReader.GetListById(70))
            {
                for(int i = 1; i < 4; i++)
                    if (Convert.ToInt32(item.GetByKey(string.Format("targets_{0}_id_to_make", i))) == id)
                        ls.Add(string.Format("List : {0} ID : {1} Name : {2} Рецепт крафта : {3}",70,
                            item.GetByKey("ID"),item.GetByKey("Name"),i));
            }
            return ls;
        }
        public static Item SearchItem(string param, int currList, out int newList, bool full , bool mat)
        {
            currList++;
            newList = currList;
            Item it = null;
            var final = !full ? currList : _elReader.Items.Count;

            for (int i = currList; i <= final; ++i)
            {
                newList = i;
                foreach (var item in _elReader.GetListById(i))
                {
                    if (item.GetByKey("ID").ToString() == param)
                    {   
                        return item;
                    }
                    if (!mat)
                    {
                        if (((string) item.GetByKey("Name")).Contains(param))
                        {
                            return item;
                        }
                    }
                    else if (((string) item.GetByKey("Name")).Replace("\0","") == param)
                    {
                        return item;
                    }


                }
            }
            return it;
        }

        public static void LoadSurfaces()
        {
            _surfaces = new List<string>();
            try
            {
                _img = Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "iconlist_ivtrf.png"));
                foreach (var line in File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "iconlist_ivtrf.txt"), Encoding.GetEncoding(936)))
                {
                    if (Path.GetExtension(line) == ".dds")
                        _surfaces.Add(line);
                }
                new Thread(CropImages).Start();
            }
            catch (Exception m)
            {
                MessageBox.Show(m.ToString());
            }


        }

        public static int[] FindCoord(string val)
        {
            int i = 0;
            int x = 0;
            int y = 0;
            if (val == null)
                return new[] {0, 0};
            val = val.Replace("\0","");
            if (i >= _surfaces.Count)
                return new[] { 0, 0 };
            while (Path.GetFileName(val) != _surfaces[i])
            {
                x += 32;
                if (x >= 4096)
                {
                    x = 0;
                    y += 32;
                }
                i++;
                if (i >= _surfaces.Count)
                {
                    return new[] {32, 0};
                }
            }
            return new [] {x, y};
        }
    }
}
