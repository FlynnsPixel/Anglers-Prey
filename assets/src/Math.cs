using UnityEngine;
using System.Collections;

public class Math {

    public const float RADIAN = Mathf.PI / 180.0f;

    public static float smooth_angle(float current_angle, float target_angle, float smooth) {
        float c = current_angle;
        bool swapped = false;
        if (target_angle > c) { float temp = target_angle; target_angle = c; c = temp; swapped = true; }
        float t1 = c - target_angle;
        float t2 = (360 - c) + target_angle;
        if (t1 < t2) target_angle = -t1;
        else if (t2 < t1) target_angle = t2;
        if (swapped) target_angle = -target_angle;

        current_angle += target_angle / smooth;
        if (current_angle < 0) current_angle += 360;
        if (current_angle > 360) current_angle -= 360;

        return current_angle;
    }
}
