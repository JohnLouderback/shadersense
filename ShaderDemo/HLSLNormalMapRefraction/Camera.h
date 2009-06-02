//////////////////////////////////////////////////////////////////////////////////////////////////
// File: Camera.h
// Author: Chris Smith
// Date Created: 2/22/06
// Description: Holds all of the camera data
// Copyright: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once
#include <d3dx9.h>

class Camera
{
public:
	Camera();
	~Camera();

	float Pitch, Yaw;
	D3DXVECTOR3 Position;
	D3DXVECTOR3 Target;
	D3DXVECTOR3 Up;
	D3DXVECTOR3 LookVector;
	D3DXMATRIX View;

	void UpdateCamera(float ForwardUnits, float SidewardUnits);
};