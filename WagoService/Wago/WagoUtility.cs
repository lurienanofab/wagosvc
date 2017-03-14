using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Net;
using System.Net.Sockets;
using LNF;
using LNF.CommonTools;
using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using LNF.Repository.Scheduler;

namespace WagoService.Wago
{
    public static class WagoUtility
    {
        //Returns the points associated with an action
        public static IList<Point> GetPoints(int pointId = 0)
        {
            using (Providers.DataAccess.StartUnitOfWork())
            {
                if (pointId == 0)
                    return DA.Current.Query<Point>().ToList();
                else
                    return new Point[] { DA.Current.Single<Point>(pointId) }.ToList();
            }
        }

        //Maps Point to memory on the block
        //Inputs: PointID, FilterDirection (Filters by Input/Output)
        //Outputs: BlockID, Word, Bit
        public static WagoAddress MapToMemory(Point point, bool filterDirection)
        {
            //Get BlockID, ModPosition, ModTypeID, and Offset

            IList<BlockConfig> configs = point.Block.Configurations;
            BlockConfig blockConfig = configs.FirstOrDefault(x => x.ModPosition == point.ModPosition);

            if (blockConfig == null)
                throw new InvalidOperationException(string.Format("Unable to get block config for BlockID = {0}, ModPosition = {1}.", point.Block.BlockID, point.ModPosition));

            //Find the specified module on the block, and get Direction, and PointSize
            ModType modType =  blockConfig.ModType;
            int intPointSize = modType.PointSize;
            int intDirection = modType.Direction;

            //Calculate mapped Word, and Bit
            IEnumerable<BlockConfig> view;
            if (filterDirection)
            {
                view = configs.Where(x => x.ModType.Direction == intDirection) //Filter by Input/Output
                    .OrderByDescending(x => x.ModType.PointSize).ThenBy(x => x.ModPosition); //Sort by Analog/Digital, then by Position
            }
            else
                view = configs.OrderBy(x => x.ModType.Direction).ThenByDescending(x => x.ModType.PointSize).ThenBy(x => x.ModPosition);

            int w = -1, b = 15; //Words and bits are 0 based
            int i;
            for (i = 0; i < view.Count(); i++)
            {
                BlockConfig bc = view.ElementAt(i);
                ModType mt = bc.ModType;

                if (bc.ModPosition == point.ModPosition)
                {
                    if (intPointSize == 16)           //2010-11 analog device offset 
                    {
                        w += point.Offset + 1;
                        b = 0;
                    }
                    else if (intPointSize == 1)
                    {
                        if (b >= 15)
                        {
                            b -= 16;
                            w += 1;
                        }
                        b += point.Offset + 1;
                    }
                    break;
                }

                if (i > 0)
                {
                    //Changing from input to output, need to increment Word and reset the Bit
                    if (mt.Direction != view.ElementAt(i - 1).ModType.Direction)
                    {
                        b = -1;
                        w += 1;
                    }
                }

                if (mt.PointSize == 16)       //2010-11 analog module
                    w += bc.ModType.NumPoints;
                else if (mt.PointSize == 1)
                {
                    if (b >= 15)
                    {
                        b -= 16;
                        w += 1;
                    }
                    b += mt.NumPoints;
                }
            }

            return new WagoAddress()
            {
                Bit = b,
                Word = w
            };
        }

        public static PointState GetPointState(Point point, byte[] buffer)
        {
            if (point == null)
                throw new ArgumentNullException("point");

            if (buffer == null)
                return point.CreatePointState(false);

            //filterDirection should be false when getting the point state
            WagoAddress addr = MapToMemory(point, false);

            int word = addr.Word;
            int bit = addr.Bit;

            int i;

            if (bit < 8)
            {
                int highByte = 2 * (word + 1) + 1;
                i = (buffer[highByte] & (int)(Math.Pow(2, bit))) >> bit;
            }
            else
            {
                int lowByte = 2 * (word + 1);
                i = (buffer[lowByte] & (int)(Math.Pow(2, (bit - 8)))) >> (bit - 8);
            }

            return point.CreatePointState(i > 0);
        }

        public static int GetAnalogPointState(Point point, byte[] data)
        {
            throw new NotImplementedException();
        }

        public static string GetDataString(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return "[none]";
            else
                return "[" + BytesToString(buffer, buffer[0]) + "]";
        }

        //Converts an array of Bytes into String for output
        public static string BytesToString(byte[] buffer, int size)
        {
            if (buffer == null) return "[null]";
            List<string> temp = new List<string>();
            for (int i = 0; i < size; i++)
                temp.Add(string.Format("{0:x2}", buffer[i]));
            string result = "0x" + string.Join(",0x", temp);
            return result;
        }
    }
}
