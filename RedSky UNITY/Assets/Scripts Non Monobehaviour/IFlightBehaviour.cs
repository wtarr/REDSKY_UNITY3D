using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IFlightBehaviour
{
    void Accelerate();
    void Decelerate();
    void PitchUp();
    void PitchDown();
    void RollLeft();
    void RollRight();
    void YawLeft();
    void YawRight();    
}
