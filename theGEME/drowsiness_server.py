import cv2
import mediapipe as mp
import numpy as np
import pygame
import socket

# Setup TCP Server
HOST = "127.0.0.1"  # Localhost
PORT = 65432  # Must match Unity
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print("Waiting for Unity connection...")

client_socket, addr = server_socket.accept()
print(f"Connected to {addr}")

# Pygame for alarm
pygame.mixer.init()
alarm_sound = pygame.mixer.Sound("alarm.mp3")

# EAR Threshold
EAR_THRESHOLD = 0.25
CONSECUTIVE_FRAMES = 80

mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(min_detection_confidence=0.5, min_tracking_confidence=0.5)

def calculate_ear(eye_landmarks):
    left_eye = [
        (eye_landmarks[362].x, eye_landmarks[362].y),
        (eye_landmarks[385].x, eye_landmarks[385].y),
        (eye_landmarks[387].x, eye_landmarks[387].y),
        (eye_landmarks[263].x, eye_landmarks[263].y),
        (eye_landmarks[373].x, eye_landmarks[373].y),
        (eye_landmarks[380].x, eye_landmarks[380].y),
    ]
    right_eye = [
        (eye_landmarks[33].x, eye_landmarks[33].y),
        (eye_landmarks[160].x, eye_landmarks[160].y),
        (eye_landmarks[158].x, eye_landmarks[158].y),
        (eye_landmarks[133].x, eye_landmarks[133].y),
        (eye_landmarks[153].x, eye_landmarks[153].y),
        (eye_landmarks[144].x, eye_landmarks[144].y),
    ]

    A = np.linalg.norm(np.array(left_eye[1]) - np.array(left_eye[5]))
    B = np.linalg.norm(np.array(left_eye[2]) - np.array(left_eye[4]))
    C = np.linalg.norm(np.array(left_eye[0]) - np.array(left_eye[3]))
    ear_left = (A + B) / (2.0 * C)

    A = np.linalg.norm(np.array(right_eye[1]) - np.array(right_eye[5]))
    B = np.linalg.norm(np.array(right_eye[2]) - np.array(right_eye[4]))
    C = np.linalg.norm(np.array(right_eye[0]) - np.array(right_eye[3]))
    ear_right = (A + B) / (2.0 * C)

    return (ear_left + ear_right) / 2.0

cap = cv2.VideoCapture(0)
frame_counter = 0

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.flip(frame, 1)
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)

    drowsy = 0

    if results.multi_face_landmarks:
        for face_landmarks in results.multi_face_landmarks:
            ear = calculate_ear(face_landmarks.landmark)
            cv2.putText(frame, f"EAR: {ear:.2f}", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            if ear < EAR_THRESHOLD:
                frame_counter += 1
                if frame_counter >= CONSECUTIVE_FRAMES:
                    drowsy = 1  # Drowsiness detected
                    if not pygame.mixer.get_busy():
                        alarm_sound.play()
                    cv2.putText(frame, "DROWSY!", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
            else:
                frame_counter = 0
                if pygame.mixer.get_busy():
                    alarm_sound.stop()

    # Send signal to Unity
    client_socket.sendall(str(drowsy).encode())

    cv2.imshow("Drowsiness Detection", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
pygame.quit()
client_socket.close()
server_socket.close()
