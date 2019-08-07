using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Helpers
{
    public static class Extensions
    {
        public static Vector2 ToVector2xz(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 ToVector3xz(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 ToVector3xyz(this Vector2 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 ModifyVector3z(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static CloudSync CloudSync()
        {
            return GameObject.FindObjectOfType<CloudSync>();
        }

        public static string CreateMD5(string input)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(input))
                ).Replace("-", String.Empty);
            }
            return hash;
        }

        public static string BoolArrayToString(bool[] arr)
        {
            string result = "";
            for(int i=0; i<arr.Length; i++)
            {
                if (arr[i])
                    result += '1';
                else
                    result += '0';
            }
            return result;
        }

        public static bool[] StringToBoolArray(string str)
        {
            bool[] result = new bool[str.Length];
            for(int i=0; i<str.Length; i++)
            {
                if (str[i] == '1')
                    result[i] = true;
                else
                    result[i] = false;
            }
            return result;
        }

        static public double DistanceInKilometers(double lat1, double lon1, double lat2, double lon2)
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return dist * 1.609344;
        }

		public static int  CalcLevenshteinDistance(string a, string b)
		{
			if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b))  return 0;

			a = a.ToUpper ();
			b = b.ToUpper ();

			int  lengthA   = a.Length;
			int  lengthB   = b.Length;
			var  distances = new int[lengthA + 1, lengthB + 1];
			for (int i = 0;  i <= lengthA;  distances[i, 0] = i++);
			for (int j = 0;  j <= lengthB;  distances[0, j] = j++);

			for (int i = 1;  i <= lengthA;  i++)
				for (int j = 1;  j <= lengthB;  j++)
				{
					int  cost = b[j - 1] == a[i - 1] ? 0 : 1;
					distances[i, j] = Math.Min
						(
							Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
							distances[i - 1, j - 1] + cost
						);
				}
			return distances[lengthA, lengthB] - (Mathf.Abs(a.Length - b.Length));
		}
    }
}
