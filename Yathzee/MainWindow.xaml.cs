using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Yathzee
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //holds the dice, the max number on the dice and what you can roll
        private Die[] DiceToRoll;

        //holds the number of dice the player starts with
        private int NumberOfStartingDice;

        //random variable
        Random rnd;

        //the damage the next attack will do
        int NextHitDamage;

        //the amount of times the player has roled in a turn
        int TimesRolledTurn = 1;

        //holds all the dice in the hold area
        private DieInHold[] DieInHoldArea;

        //EnemyHealth
        int EnemyHealth;


        public MainWindow()
        {
            InitializeComponent();

            NewGame();
        }

        private void NewGame()
        {
            //set the health of the first enemy
            EnemyHealth = 10;


            //set rnd to an random seed
            rnd = new Random();

            //set the number of dice the player starts with
            NumberOfStartingDice = 3;

            //create an array with empty dice
            DiceToRoll = new Die[10];
            DieInHoldArea = new DieInHold[5];


            //disable all hold slots
            DiceHoldGroup.Children.Cast<Button>().ToList().ForEach(button =>
            {
                button.IsEnabled = false;
                button.Content = "";
            });


            //Create disabled emtpy dice in hold
            for (int i = 0; i < DieInHoldArea.Count(); i++)
            {
                DieInHoldArea[i] = new DieInHold(0, false, false);
            }


            //disable all buttons exept a number of dice equal to the starting dice, give to still enabled basic dice data
            for (int i = 0; i < DiceToRoll.Length; i++)
            {
                Button button = (Button)DiceToRollGroup.FindName("DiceToRoll" + "_" + (i).ToString());
                if (i < NumberOfStartingDice)
                {
                    button.IsEnabled = true;
                    DiceToRoll[i] = new Die(0, new List<string> { "N.0", "A.1", "A.1", "A.1", "A.2", "A.3" }, 6);
                } else
                {
                    button.IsEnabled = false;
                    DiceToRoll[i] = new Die(0, new List<string>(), 0);
                }
            }

            bool HasRoled = RollAllDice();
        }
        /// <summary>
        /// 
        /// take a die that is enabled and its maximum number and create a random number with a max to that number.
        /// with that number select the face of the dice and dsiplay it.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RollDiceClick(object sender, RoutedEventArgs e)
        {
            Button Sender = (Button)sender;
            bool HasRoled = RollAllDice();


            UpdateDiceRoll(HasRoled);

        }

        public void UpdateDiceRoll(bool HasRoled)
        {
            if (HasRoled)
            {
                TimesRolledTurn++;
                RollLabel.Content = TimesRolledTurn.ToString() + "/3";
                if (TimesRolledTurn >= 3)
                {
                    RollDice.IsEnabled = false;
                }
                else{
                    RollDice.IsEnabled = true;
                }
            }

            for (int i = 0; i < DieInHoldArea.Count(); i++)
            {
                if (DieInHoldArea[i] != null)
                {
                    DieInHoldArea[i].JustMoved = false;
                }
            }
        }
        /// <summary>
        /// Take every button in the to roll zone and genrerate a random number for them
        /// if there was no dice in the to roll zone then return false otherwise retrun true
        /// </summary>
        /// <returns></returns>
        public bool RollAllDice()
        {

            bool HasRoled = false;
            DiceToRollGroup.Children.Cast<Button>().ToList().ForEach(button =>
            {
                if (button.IsEnabled)
                {
                    HasRoled = true;
                    int DieToRoll = int.Parse(button.Name.Split('_')[1]);
                    int Roll = rnd.Next(DiceToRoll[DieToRoll].Numbers);
                    Debug.Write(Roll.ToString());
                    DiceToRoll[DieToRoll].Number = int.Parse(DiceToRoll[DieToRoll].GetFace(Roll).Split('.')[1]);
                    Debug.Print(DiceToRoll[DieToRoll].Number.ToString());
                    button.Content = DiceToRoll[DieToRoll].GetFace(Roll);
                }
            });
            return HasRoled;
        }

        /// <summary>
        /// Take The button That was pressed and disbale it and eneable the corrospoonding button in the hold zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiceToHold(object sender, RoutedEventArgs e)
        {
            Button Sender = (Button)sender;

            bool FoundHoldSpace = false;

            DiceHoldGroup.Children.Cast<Button>().ToList().ForEach(button =>
            {
                if (!button.IsEnabled && !FoundHoldSpace)
                {
                    DieInHoldArea[int.Parse(button.Name.Split('_')[1])] = new DieInHold(int.Parse(Sender.Name.Split('_')[1]),true,true);
                    FoundHoldSpace = true;
                    button.IsEnabled = true;
                    button.Content = Sender.Content;
                }
            });

            Sender.IsEnabled = false;
            UpdateDiceMove();
        }


        /// <summary>
        /// Take The button That was pressed and disable it and enable the corrospoonding button in the roll zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiceFromHold(object sender, RoutedEventArgs e)
        {
            
            Button Sender = (Button)sender;
            int DiceNumber = int.Parse(Sender.Name.Split('_')[1]);
            Debug.Print(DieInHoldArea[DiceNumber].JustMoved.ToString());
            if (DieInHoldArea[DiceNumber].JustMoved) {
                Sender.IsEnabled = false;
                Sender.Content = "";
                Button button = (Button)DiceToRollGroup.FindName("DiceToRoll" + "_" + (DieInHoldArea[DiceNumber].RollPos).ToString());
                button.IsEnabled = true;
                DieInHoldArea[DiceNumber].Enabled = false;
            }
            UpdateDiceMove();
        }


        /// <summary>
        /// Take the number of dice in the hold area and loop that many times each loop adding the damage of the dice to the damege for each of the same dice.
        /// </summary>
        private int UpdateDiceMove()
        {

            NextHitDamage = 0;

            for (int i = 0; i < DieInHoldArea.Count(); i++)
            {
                
                foreach (DieInHold ii in DieInHoldArea)
                {
                    if (DiceToRoll[ii.RollPos].Number == DiceToRoll[DieInHoldArea[i].RollPos].Number && ii.Enabled && DieInHoldArea[i].Enabled)
                    {
                        NextHitDamage += DiceToRoll[DieInHoldArea[i].RollPos].Number;
                        Console.WriteLine(NextHitDamage.ToString() + "Damage");
                    }

                }
            }

            DamageLabel.Content = NextHitDamage.ToString();

            return NextHitDamage;
        }


        /// <summary>
        /// Deal the damage of the dice
        /// then disable all dice in hold area and enable the available dice in the to roll zone 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextTurnClick(object sender, RoutedEventArgs e)
        {
            EnemyHealth -= UpdateDiceMove();

            EnemyHPLabel.Content = "Enemy HP: " + EnemyHealth.ToString();

            DiceHoldGroup.Children.Cast<Button>().ToList().ForEach(button =>
            {
                if (button.IsEnabled)
                {
                    int DieToDisable = int.Parse(button.Name.Split('_')[1]);
                    DieInHoldArea[DieToDisable] = new DieInHold(0, false,false);
                    button.IsEnabled = false;
                    button.Content = "";
                }
            });

            for (int i = 0; i < DiceToRoll.Length; i++)
            {
                Button button = (Button)DiceToRollGroup.FindName("DiceToRoll" + "_" + (i).ToString());
                if (i < NumberOfStartingDice)
                {
                    button.IsEnabled = true;
                    DiceToRoll[i] = new Die(0, new List<string> { "N.0", "A.1", "A.1", "A.1", "A.2", "A.3" }, 6);
                }
                else
                {
                    button.IsEnabled = false;
                    DiceToRoll[i] = new Die(0, new List<string>(), 0);
                }
            }

            TimesRolledTurn = 0;

            UpdateDiceRoll(RollAllDice());

        }
    }

    public class DieInHold
    {
        public int RollPos { get; set; }
        public bool JustMoved{ get; set; }
        public bool Enabled { get; set; }

        public DieInHold(int rollPos, bool justMoved, bool enabled)
        {
            Enabled = enabled;
            RollPos = rollPos;
            JustMoved = justMoved;
        }
    }

    public class Die
    {
        public int Number { get; set; }
        public int Numbers { get; set; }

        public List<string> Faces;

        public Die(int number, List<string> faces, int numbers)
        {
            Number = number;
            Faces = faces;
            Numbers = numbers;
        }

        public string GetFace(int Face)
        {
            return Faces[Face];
        }
    }
}
