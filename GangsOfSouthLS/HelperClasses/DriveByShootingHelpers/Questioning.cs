﻿using GangsOfSouthLS.HelperClasses.CommonUtilities;
using GangsOfSouthLS.INIFile;
using GangsOfSouthLS.Menus;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GangsOfSouthLS.HelperClasses.DriveByShootingHelpers
{
    internal class Questioning
    {
        internal SuspectCarTemplate CarTemplate;
        internal EInformation CollectedInfo;
        internal EQuestioningQuality Quality
        {
            get { return GetQuestioningQuality(); }
        }

        internal bool IsFinished
        {
            get { return (WitnessDict.Count == 0); }
        }

        internal Dictionary<MyPed, Blip> WitnessDict { get; private set; }
        private Dictionary<MyPed, Answer> PedAnswerDict;

        private List<Answer> PossibleAnswers = new List<Answer>
        {
            new Answer( "I came as fast as I could when I heard the shots, to see if anyone needed help but the car was long gone when I arrived.", EInformation.None ),
            new Answer( "I didn't see anything, officer. I wasn't here yet when the shots were fired.", EInformation.None ),
            new Answer( "It was all so hectic, I could only see that the car was {0}. That's all. Sorry, officer.", EInformation.Color ),
            new Answer( "All I could make out was {0} car that drove away as fast as it came.", EInformation.Color ),
            new Answer( "I saw a guy in a {0} {1} shooting at these people, but I wasn't close enough to catch the plate.", EInformation.Color | EInformation.Class ),
            new Answer( "I saw the car! It was a {0} {1}! Please catch the bastards who did this, officer!", EInformation.Color | EInformation.Class ),
            new Answer( "I heard shots, and then I saw a {0} {1} speeding away. I'm sure it was the shooters!", EInformation.Color | EInformation.Model | EInformation.Class ),
            new Answer( "I was walking down the street and then all of a sudden a guy in a {0} {1} started shooting!", EInformation.Color | EInformation.Model | EInformation.Class ),
            new Answer( "I saw the license plate! It was {0}. Surely you can catch those guys now, right?", EInformation.Plate ),
            new Answer( "There were shots and screams! Fortunately I was lucky enough to catch a glimpse of the plate. It was {0}.", EInformation.Plate ),
        };

        private List<Answer> ChosenAnswers;

        [Flags]
        internal enum EInformation
        {
            None = 0,
            Color = 1,
            Model = 2,
            Class = 4,
            Plate = 8,
            Address = 16,
        }

        internal enum EQuestioningQuality
        {
            Useless, Ambiguous, Definite
        }

        internal Questioning(List<MyPed> witnessList, SuspectCarTemplate carTemplate)
        {
            CarTemplate = carTemplate;
            PedAnswerDict = new Dictionary<MyPed, Answer> { };
            WitnessDict = new Dictionary<MyPed, Blip> { };
            ChosenAnswers = new List<Answer> { };
            CollectedInfo = 0;
            DriveByMenu.InformationDict = new Dictionary<string, string> { };
            foreach (var wit in witnessList)
            {
                var randomAnswer = PossibleAnswers.Except(ChosenAnswers).ToArray().RandomElement();
                ChosenAnswers.Add(randomAnswer);
                PedAnswerDict.Add(wit, randomAnswer);
                var blip = new Blip(wit);
                blip.Color = Color.Orange;
                blip.Scale = 0.75f;
                blip.Order = 1;
                WitnessDict.Add(wit, blip);
            }
            GameFiber.StartNew(delegate
            {
                while (!IsFinished)
                {
                    Game.DisplayHelp("Press ~b~Y ~w~while standing close to a ~o~witness ~w~to get their statement. Or press ~b~" + INIReader.MenuKeyString + " ~w~to review the information you collected.");
                    if (Game.IsKeyDown(Keys.Y))
                    {
                        var closestUnquestionedWitness = (MyPed)World.GetClosestEntity(WitnessDict.Keys.ToArray(), Game.LocalPlayer.Character.Position);
                        if (Game.LocalPlayer.Character.DistanceTo(closestUnquestionedWitness) < 2.5f)
                        {
                            PlayAnswer(closestUnquestionedWitness);
                        }
                    }
                    GameFiber.Yield();
                }
            });
        }

        private void PlayAnswer(MyPed witness)
        {
            var answer = PedAnswerDict[witness];
            answer.Play(CarTemplate);
            CollectedInfo |= answer.Information;
            WitnessDict[witness].SafelyDelete();
            WitnessDict.Remove(witness);
            foreach (EInformation info in Enum.GetValues(typeof(EInformation)))
            {
                if (answer.Information.HasFlag(info) && !DriveByMenu.InformationDict.Keys.Contains(info.ToString()))
                {
                    if (info == EInformation.Color)
                    {
                        DriveByMenu.InformationDict.Add(info.ToString(), CarTemplate.ColorString);
                    }
                    if (info == EInformation.Class)
                    {
                        DriveByMenu.InformationDict.Add(info.ToString(), CarTemplate.VehClass);
                    }
                    if (info == EInformation.Model)
                    {
                        DriveByMenu.InformationDict.Add(info.ToString(), CarTemplate.VehModel);
                    }
                    if (info == EInformation.Plate)
                    {
                        DriveByMenu.InformationDict.Add(info.ToString(), CarTemplate.LicensePlate);
                    }
                    if (info == EInformation.Address)
                    {
                        DriveByMenu.InformationDict.Add(info.ToString(), CarTemplate.Address);
                    }
                }
            }
        }

        private EQuestioningQuality GetQuestioningQuality()
        {
            if (CollectedInfo.HasFlag(EInformation.Plate))
            {
                return EQuestioningQuality.Definite;
            }
            else if (CollectedInfo.HasFlag(EInformation.Class))
            {
                return EQuestioningQuality.Ambiguous;
            }
            else
            {
                return EQuestioningQuality.Useless;
            }
        }

        internal string GetDescription()
        {
            var description = CarTemplate.ColorString + " ";
            if (CollectedInfo.HasFlag(EInformation.Model))
            {
                return description += CarTemplate.VehModel;
            }
            else if (CollectedInfo.HasFlag(EInformation.Class))
            {
                return description += CarTemplate.VehClass;
            } 
            else
            {
                return description += "car";
            }
        }


        private class Answer
        {
            internal string AnswerString;
            internal EInformation Information;

            internal Answer(string answerString, EInformation information)
            {
                AnswerString = answerString;
                Information = information;
            }

            internal void Play(SuspectCarTemplate carTemplate)
            {
                var subtitle = "~o~Witness~w~: ";
                if (Information == EInformation.Color)
                {
                    subtitle += string.Format(AnswerString, carTemplate.ColorString);
                }
                else if (Information == (EInformation.Color | EInformation.Class))
                {
                    subtitle += string.Format(AnswerString, carTemplate.ColorString, carTemplate.VehClass);
                }
                else if (Information == (EInformation.Color | EInformation.Model | EInformation.Class))
                {
                    subtitle += string.Format(AnswerString, carTemplate.ColorString, carTemplate.VehModel);
                }
                else if (Information == EInformation.Plate)
                {
                    subtitle += string.Format(AnswerString, carTemplate.LicensePlate);
                }
                else if (Information == EInformation.None)
                {
                    subtitle += AnswerString;
                }
                Game.DisplaySubtitle(subtitle);
            }
        }
    }
}