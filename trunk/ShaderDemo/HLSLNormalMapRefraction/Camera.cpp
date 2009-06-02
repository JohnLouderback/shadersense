//////////////////////////////////////////////////////////////////////////////////////////////////
// File: Camera.cpp
// Author: Chris Smith
// Date Created: 2/22/06
// Description: Holds all of the camera data
// Copyright: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////

#include "Camera.h"

///////////////////////////////////////////////////////
// Camera Constructor
///////////////////////////////////////////////////////
Camera::Camera()
{
	Pitch = 0.0f;
	Yaw = 1.54f;
	Position = D3DXVECTOR3(1.0f, 6.0f, -20.0f);
	Target = D3DXVECTOR3(0.0f, 6.0f, 0.0f);
	Up = D3DXVECTOR3(0.0f, 1.0f, 0.0f);
	D3DXVec3Normalize(&LookVector, &(Target - Position));
}

///////////////////////////////////////////////////////
// Camera Deconstructor
///////////////////////////////////////////////////////
Camera::~Camera()
{
	//Nothing to do here
}

///////////////////////////////////////////////////////
// Update the Camera
///////////////////////////////////////////////////////
void Camera::UpdateCamera(float ForwardUnits, float SidewardUnits)
{
	//Restrict the ability to look too high or too low
	if( Pitch < -1.56f )
		Pitch = -1.56f;
	if( Pitch > 1.56f )
		Pitch = 1.56f;

	if( Yaw >= 6.28f )
		Yaw = 0.0f;
	if( Yaw <= -6.28f)
		Yaw = 0.0f;

	//Set the target of the camera
	Target = D3DXVECTOR3( cosf(Pitch) * cosf(Yaw) * 10.0f, sinf(Pitch) * 10.0f, sinf(Yaw) * cosf(Pitch) * 10.0f) + Position;

	//Update the Look Vector
	D3DXVec3Normalize(&LookVector, &(Target - Position));

	//We do not want to move up or down so we zero the Y variable and only
	//move in the X and Z directions
	D3DXVECTOR3 XZLookVector = LookVector;
	XZLookVector.y = 0;
	D3DXVec3Normalize(&XZLookVector, &XZLookVector);
	D3DXVECTOR3 SideVector( XZLookVector.z, 0.0f, -XZLookVector.x );
	Position += (XZLookVector * ForwardUnits) + (SideVector * SidewardUnits);
	Target += (XZLookVector * ForwardUnits) + (SideVector * SidewardUnits);

	//Update the View matix
	D3DXMatrixLookAtLH(&View, &Position, &Target, &Up);
}