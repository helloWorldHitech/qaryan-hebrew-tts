﻿//    This file is part of Qaryan.
//
//    Qaryan is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Qaryan is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Qaryan.  If not, see <http://www.gnu.org/licenses/>.

/*
 * Created by SharpDevelop.
 * User: Moti Z
 * Date: 8/25/2007
 * Time: 9:38 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections.Generic;
using Qaryan.Core;
using MotiZilberman;

namespace Qaryan.Core
{
	/// <summary>
	/// Description of Phonetizer.
	/// </summary>
	public class Phonetizer: LookaheadConsumerProducer<Segment,Phone>
	{
//		int bpm;
		bool firstStressInClause=true;
		
		public struct ContextOptions {
			public bool Akanye,Ikanye,DistinguishStrongDagesh,SingCantillation;
		}
		
		public ContextOptions Options=new ContextOptions();
		
		/*double MidiNotePitch(byte note) {
			double n=note-69;
			double p= 440 * Math.Pow((double)2,n/12);
			return p;
		}*/
		
		protected override void BeforeConsumption()
		{
			Console.WriteLine("phonetizer started");
			base.BeforeConsumption();
		firstStressInClause=true;

			Phone phn=new Phone("_",1);
			Emit(phn);
			Console.WriteLine("phonetizer: {0}",phn);
		}
		
		protected override void Consume(Queue<Segment> InQueue)
		{
			if (InQueue.Count==0)
				return;
			Segment seg=InQueue.Dequeue();
            _ItemConsumed(seg);
			Segment nextSeg=null;
			if (InQueue.Count>0)
				nextSeg=InQueue.Peek();
			if (seg is SeparatorSegment) {
				if (HebrewChar.IsPunctuation((seg[0] as Separator).Latin[0])) {
					Phone phn=Phone.Create(seg[0]);
					if (phn==null)
						phn=new Phone("_",1);
					Emit(phn);
					Console.WriteLine("phonetizer: {0}",phn);

				}
				firstStressInClause=true;
			}
			else if (seg is Word) {
				Word w=(Word)seg;

				bool beforeStress=true;
				SpeechElement HintStrongDagesh=null;
				foreach (Syllable syl in w.Syllables) {
					
					bool stressed=syl.IsStressed;
					if (beforeStress&&stressed)
						beforeStress=false;
					bool beforeNucleus=true;
					
//					bool sylStart=true;
					foreach (SpeechElement e in	w.Phonemes.GetRange(syl.Start,syl.End-syl.Start+1)) {
						Phone phone=null;
						bool sylEnd=(syl.Phonemes.IndexOf(e)==syl.Phonemes.Count-1);
						if ((e is Consonant) && ((((Consonant)e).Flags&ConsonantFlags.StrongDagesh)!=0) && (e==HintStrongDagesh)) {
							continue;
						}
						
						phone=Phone.Create(e);
						if (phone==null)
							continue;
						phone.Context.IsNucleus=(syl.Nucleus==e);
						phone.Context.IsAccented=stressed;
                        if (nextSeg is SeparatorSegment)
                        {
                            phone.Context.NextSeparator = (nextSeg as SeparatorSegment)[0].Latin;
                        }
						if (!stressed) {
							if (Options.Akanye)
								if (phone.Symbol=="o")
								phone.Symbol="a";
							if (Options.Ikanye)
								if (phone.Symbol=="e")
								phone.Symbol="i";
						}
						else
							phone.Context.AccentStrength=1;
						if (phone.PitchCurve.Count>0)
							phone.PitchCurve.Clear();
											
						phone.Duration=80;
						if (e is Vowel) {
							Vowel v=(Vowel)e;
							if (v.IsVowelIn(Vowels.VeryShort))
								phone.Duration=28; //38
							else if (v.IsVowelIn(Vowels.Short))
								phone.Duration=90;
							else if (v.IsVowelIn(Vowels.Long))
								phone.Duration=94;
							else if (v.IsVowelIn(Vowels.VeryLong))
								phone.Duration=97;
							if (v.IsVowelIn(Vowels.HighVowels))
								phone.Duration+=25;
							else
								phone.Duration+=30;
						}
						else if (e is Consonant)
							phone.Duration=(((Consonant)e).Sonority)*2.6+60;
						else
							phone.Duration=100;
						
						if (stressed) {
							if (firstStressInClause) {
								firstStressInClause=false;
							}
							if (e==syl.Nucleus)
								beforeNucleus=false;
							phone.Duration*=1;
							if (beforeNucleus && (e is Consonant) && ((Consonant)e).IsLiquid)
								phone.Duration*=1.2;
							
						}
						else {
							if (e is Vowel) {
								if (beforeStress)
									
									phone.Duration*=0.5;
								else
									phone.Duration*=0.6;
							}
						}
						if ((e is Consonant) && ((((Consonant)e).Flags&ConsonantFlags.StrongDagesh)!=0)) {
							HintStrongDagesh=e;
							if (Options.DistinguishStrongDagesh)
								phone.Duration*=1.4;
							else
								phone.Duration*=1.1F;
						}

							Emit(phone);
							Console.WriteLine("phonetizer: {0}",phone);

						

						
//						sylStart=false;
					}
				}
				/*					if (w.CantillationMarks.Contains('֑'))
						silpr.Phones.Add(new Phone("_",200));*/
				#region cantillation stuff
				if (Options.SingCantillation)
					foreach (char ch in w.CantillationMarks) {
					int i=HebrewChar.DisjunctiveRank(ch),len=0;
					
					if (i<5) {
						switch (i) {
							case 1:
								len=230;
								break;
							case 2:
								len=165;
								break;
							case 3:
								len=60;
								break;
							case 4:
								len=30;
								break;
						}
						Phone phone;
						Emit(phone=new Phone("_",len));
						Console.WriteLine("phonetizer: {0}",phone);
					}

					
				}
				#endregion
			}
			
		}
		
		protected override void AfterConsumption()
		{
			base.AfterConsumption();
			Phone phn=new Phone("_",1);
			Emit(phn);
			Console.WriteLine("phonetizer: {0}",phn);
            _DoneProducing();
			Console.WriteLine("phonetizer finished");
		}
		
		public override void Run(Producer<Segment> producer)
		{
			base.Run(producer,2);
		}
		
		public Phonetizer()
		{
		}
	}
}
