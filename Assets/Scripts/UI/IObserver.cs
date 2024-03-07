using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserver
{
    // classes inherited from this interface
    // must implement OnNotify method
    public void OnNotify()
    {
        // do the thing when the even happens
    }
}
