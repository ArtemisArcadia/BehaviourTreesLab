using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {
        private Root CreateBehaviourTree() {

            switch (m_Behaviour) {

			case 0:
				return Fun ();
            case 1:
                return SpinBehaviour(-0.05f, 1f);
            case 2:
				return Cowardly();
			case 3:
				return Wild();

                default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }

        private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

		private Node StopMoving(){
			return new Action (() => Move (0));
		}


        /* Example behaviour trees */

        // Constantly spin and fire on the spot 
        private Root SpinBehaviour(float turn, float shoot) {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        }

        // Turn to face your opponent and fire
        private Root TrackBehaviour() {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        new Wait(2f),
                                        RandomFire())),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(0.2f))),
                            // Turn left toward target
                            new Action(() => Turn(-0.2f))
                    )
                )
            );
        }

		private Root Wild() {
			return new Root (
				new Service (0.2f, UpdatePerception,
					new Selector (
						new BlackboardCondition ("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 10f,
							Stops.IMMEDIATE_RESTART,
							new Sequence(
								new Action(() => Move(1)),
								new Action(() => Turn(1)),
								new Action(() => Turn(-1)),
								new Action(() => Fire(0))
							)
						),
						new BlackboardCondition ("targetDistance",
							Operator.IS_GREATER_OR_EQUAL, 11f,
							Stops.IMMEDIATE_RESTART,
							StopMoving()
						)
					)
				)
						
			);
		}



			


		//follows when close and fires, evades
		private Root Fun()
		{


			return new Root (
				new Service (0.2f, UpdatePerception,
					new Selector (
						new BlackboardCondition("objectBlock", Operator.IS_EQUAL,true,
							Stops.IMMEDIATE_RESTART,
							new Sequence(
								new Action(() => Turn(1)),
								new Wait(0.2f),
								StopTurning())),

						new BlackboardCondition("objectBehind", Operator.IS_EQUAL,true,
							Stops.IMMEDIATE_RESTART,
							new Sequence(
								new Action(() => Move(0.1f)),
								new Wait(1f),
								new Action(() => Turn(1)),
								new Wait(0.1f),
								StopTurning())),
						
						new BlackboardCondition ("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,

							new Sequence (
							 StopTurning (),
								new Action (() => Fire (0.5f)))),

						new BlackboardCondition ("targetDistance",
							Operator.IS_GREATER_OR_EQUAL, 10f,
							Stops.IMMEDIATE_RESTART,

							
								new Action (() => Move (0.5f))),

						new BlackboardCondition ("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 3f,
							Stops.IMMEDIATE_RESTART,

							new Action (() => Move (0.5f))),


						new BlackboardCondition ("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,


							// Turn right toward target
							new Action (() => Turn (1))),
						// Turn left toward target
						new Action (() => Turn (-1))




					)
				
				)
			);
		}

		//Runs away

		private Root Cowardly()
		{


			return new Root (
				new Service (0.2f, UpdatePerception,
					new Selector (
						new BlackboardCondition("objectBlock", Operator.IS_EQUAL,true,
							Stops.IMMEDIATE_RESTART,
							new Sequence(
								new Action(() => Move(-1)),
								new Action(() => Turn(2)),
								new Action(() => StopTurning()))),

						new BlackboardCondition("objectBehind", Operator.IS_EQUAL,true,
							Stops.IMMEDIATE_RESTART,
							new Sequence(
								new Action(() => Move(1)),
								new Action(() => Turn(2)),
								new Action(() => StopTurning()))),

						new BlackboardCondition ("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,

							new Sequence (
								new Action (() => StopTurning ()),
								new Action (() => Fire (1)))),

						new BlackboardCondition ("targetDistance",
							Operator.IS_GREATER_OR_EQUAL, 30f,
							Stops.IMMEDIATE_RESTART,

							new Action (() => StopMoving())),

						new BlackboardCondition ("targetDistance",
							Operator.IS_SMALLER_OR_EQUAL, 30f,
							Stops.IMMEDIATE_RESTART,

							new Action (() => Move (-1))),
						


						new BlackboardCondition ("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,


							// Turn left away target
							new Action (() => Turn (1))),
						// Turn right
						 	new Action (() => Turn (1))




					)

				)
			);
		}

        private void UpdatePerception() {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;
			Vector3 Front = transform.TransformDirection (Vector3.forward); //variable that holds Vector3 forward value
			Vector3 Behind = transform.TransformDirection (Vector3.back); //variable that holds Vector 3 back value
			bool objectBlocking = Physics.Raycast (transform.position, Front, 3); //checks to see if something is close in front of it
			bool objectBehind = Physics.Raycast (transform.position, Behind, 1); //checks to see if something is close behind it
            
			blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
			blackboard ["objectBlock"] = objectBlocking; //if something is behind it, returns true
			blackboard ["objectBehind"] = objectBehind; //if something is behind it, returns true

        }

    }
}