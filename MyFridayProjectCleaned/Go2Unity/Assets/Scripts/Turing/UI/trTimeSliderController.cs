using UnityEngine;
using UnityEngine.UI;

namespace Turing
{
	public class trTimeSliderController : trParaSliderController
	{
		protected override string GetLabelValue(float value) {
			return trStringFactory.GetTimeText(value);
		}
	}
}

