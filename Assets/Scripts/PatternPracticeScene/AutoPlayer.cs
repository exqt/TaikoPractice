public class AutoPlayer
{
    readonly InputThread inputThread;
    readonly SongPlay songPlay;
    readonly PatternPracticeScene scene;
    static readonly NoteType[] _noteTypes = new[] { NoteType.Don, NoteType.Ka };

    public bool enabled = false;
    HandType lastHand = HandType.None;

    public AutoPlayer(
        InputThread inputThread,
        SongPlay songPlay,
        PatternPracticeScene scene
    ) {
        this.inputThread = inputThread;
        this.songPlay = songPlay;
        this.scene = scene;

        inputThread.OnUpdate += OnInputThreadUpdate;
    }

    void OnInputThreadUpdate()
    {
        if (
            !enabled ||
            scene.StateType != PatternPracticeScene.GameStateType.Playing
        ) return;

        var curTime = scene.CurrentTime;
        songPlay.SetTime(curTime);

        HandType hand = HandType.None;
        if (lastHand == HandType.None || lastHand == HandType.Left)
            hand = HandType.Right;
        else if (lastHand == HandType.Right)
            hand = HandType.Left;

        foreach (NoteType noteType in _noteTypes)
        {
            var _note = songPlay.GetNoteToHit(noteType, curTime);
            if (_note == null) continue;

            var note = _note.Value;

            if (curTime < note.time) continue;

            TaikoKeyType keyType = note.type.ToTaikoKeyType(hand);
            scene.HitNote(keyType);

            lastHand = hand;
        }
    }
}
