
function Update ()
{
	//if(!animation.IsPlaying("walk"))
	//{
		//animation.Play("walk");
		if (Input.GetAxis("Vertical") > 0.2)
		{   
		//	animation.CrossFade ("sitIdle2standIdle");
		//	animation.PlayQueued("standIdle2walk");
			animation.Play("walk");
		}
		else
		{
			if(!animation.IsPlaying("walk"))
			{
				animation.Play("walk");
			}
			
		}
	//}
} 