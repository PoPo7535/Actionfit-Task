using System.Collections.Generic;
using UnityEngine;

struct WallTransformData
{
    public Vector3 posOffset;
    public Quaternion rotation;
    public bool addInfo;
    public DestroyWallDirection destroyDir;

    private WallTransformData(Vector3 posOffset, Quaternion rotation, bool addInfo , DestroyWallDirection destroyDir)
    {
        this.posOffset = posOffset;
        this.rotation = rotation;
        this.addInfo = addInfo;
        this.destroyDir = destroyDir;
    }

    public static readonly Dictionary<ObjectPropertiesEnum.WallDirection, WallTransformData> wallTransformTable =
        new()
        {
            {
                ObjectPropertiesEnum.WallDirection.Single_Up,
                new WallTransformData(new Vector3(0f, 0f, 0.5f), Quaternion.Euler(0f, 180f, 0f), true,
                    DestroyWallDirection.Up)
            },
            {
                ObjectPropertiesEnum.WallDirection.Single_Down,
                new WallTransformData(new Vector3(0f, 0f, -0.5f), Quaternion.identity, true,
                    DestroyWallDirection.Down)
            },
            {
                ObjectPropertiesEnum.WallDirection.Single_Left,
                new WallTransformData(new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), true,
                    DestroyWallDirection.Left)
            },
            {
                ObjectPropertiesEnum.WallDirection.Single_Right,
                new WallTransformData(new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), true,
                    DestroyWallDirection.Right)
            },
            {
                ObjectPropertiesEnum.WallDirection.Left_Up,
                new WallTransformData(new Vector3(-0.5f, 0f, 0.5f), Quaternion.Euler(0f, 180f, 0f), false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Left_Down,
                new WallTransformData(new Vector3(-0.5f, 0f, -0.5f), Quaternion.identity, false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Right_Up,
                new WallTransformData(new Vector3(0.5f, 0f, 0.5f), Quaternion.Euler(0f, 270f, 0f), false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Right_Down,
                new WallTransformData(new Vector3(0.5f, 0f, -0.5f), Quaternion.Euler(0f, 0f, 0f), false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Open_Up,
                new WallTransformData(new Vector3(0f, 0f, 0.5f), Quaternion.Euler(0f, 180f, 0f), false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Open_Down,
                new WallTransformData(new Vector3(0f, 0f, -0.5f), Quaternion.identity, false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Open_Left,
                new WallTransformData(new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), false,
                    DestroyWallDirection.None)
            },
            {
                ObjectPropertiesEnum.WallDirection.Open_Right,
                new WallTransformData(new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), false,
                    DestroyWallDirection.None)
            },
        };
}