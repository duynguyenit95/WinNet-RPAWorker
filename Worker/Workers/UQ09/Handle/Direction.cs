using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using UQ09.Enum;

namespace UQ09.Handle
{
    public class Direction
    {
        
        public UQAction WhatDo()
        {
            var (typ, act) = Act();
            if (typ != -1)
            {
                return (UQAction)typ;
            }
            var now = DateTime.Now;
            if (now.DayOfWeek > DayOfWeek.Monday)
            {
                return UQAction.Nothing;
            }
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                Update(UQAction.CheckFile);
                return UQAction.Nothing;
            }
            if (now.DayOfWeek == DayOfWeek.Monday)
            {
                var target = DateTime.Today.AddHours(16).AddMinutes(30);
                var deadline = DateTime.Today.AddHours(17);
                if (now >= DateTime.Today.AddHours(8) && now<= target)
                {
                    if(act != (int)UQAction.CheckFile)
                    {
                        return UQAction.Nothing;
                    }
                    else
                    {
                        Update(UQAction.CheckFormat);
                        return UQAction.CheckFile;
                    }
                }
                if (now >= target && now <= deadline)
                {
                    if(act != (int) UQAction.CheckFormat)
                    {
                        return UQAction.Nothing;
                    }
                    else
                    {
                        Update(UQAction.Nothing);
                        return UQAction.CheckFormat;
                    }
                }
                else
                {
                    return UQAction.Nothing;
                }
            }
            else
            {
                return UQAction.Nothing;
            }
        }
        public (int,int) Act()
        {
            string file = Path.Combine(AppContext.BaseDirectory, "Parameter", "Action.txt");
            var text = File.ReadAllLines(file);
            return (Int32.Parse(text[0]), Int32.Parse(text[1]));
        }
        public void Update(UQAction act)
        {
            int actValue = (int)act;
            string file = Path.Combine(AppContext.BaseDirectory, "Parameter", "Action.txt");
            var lines = File.ReadAllLines(file);

            if (lines.Length > 1)
            {
                lines[1] = actValue.ToString();
            }
            File.WriteAllLines(file, lines);
        }
    }
}
