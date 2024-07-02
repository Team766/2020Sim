#!/usr/bin/env python3

import argparse
import ctypes
import ctypes.util
import os.path
import signal
import subprocess
import sys

FILES_DIR = os.path.dirname(__file__)

# Constant taken from http://linux.die.net/include/linux/prctl.h
PR_SET_PDEATHSIG = 1

class PrCtlError(Exception):
    pass

def set_parent_exit_signal():
    """
    Return a function to be run in a child process which will trigger SIGNAME
    to be sent when the parent process dies
    """
    # http://linux.die.net/man/2/prctl
    libc = ctypes.CDLL(ctypes.util.find_library("c"), use_errno=True)
    if libc.prctl(PR_SET_PDEATHSIG, signal.SIGABRT) != 0:
        errno = ctypes.get_errno()
        raise OSError(errno, f"SET_PDEATHSIG prctl failed: {os.strerror(errno)}")

class TerminatingPopen(subprocess.Popen):
    def __exit__(self, exc_type, value, traceback) -> None:
        self.terminate()
        return super().__exit__(exc_type, value, traceback)

def main(argv=None):
    parser = argparse.ArgumentParser()
    parser.add_argument("project_base")
    parser.add_argument("jar_file")
    args = parser.parse_args(argv)

    print("\nView simulation at http://localhost:8000\n")

    with (
        TerminatingPopen(
            [f"{FILES_DIR}/sim/server.x86_64"],
            stdout=open("sim_server.log", "w"),
            stderr=subprocess.STDOUT,
            preexec_fn=set_parent_exit_signal,
        ) as game_server,
        TerminatingPopen(
            [sys.executable, f"{FILES_DIR}/sim_server.py"],
            stdout=open("http_server.log", "w"),
            stderr=subprocess.STDOUT,
            preexec_fn=set_parent_exit_signal,
        ) as http_server,
        TerminatingPopen(
            ["java", "-cp", args.jar_file, "com.team766.hal.simulator.RobotMain", "-vr_connector"],
            cwd=args.project_base,
            preexec_fn=set_parent_exit_signal,
        ) as robot_code,
    ):
        game_server.wait()
        http_server.wait()
        robot_code.wait()

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        pass