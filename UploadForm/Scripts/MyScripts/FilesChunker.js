var TotalProg = 0;
var CurrentProg = 0;
var startProg = 0;
var progress = 0;
function Upload (UploadURL, getStateURL, UseResume, FileName,ProgressCallBack) {
    var start = 0;
    TotalProg = 0;
    CurrentProg = 0;
    startProg = 0;
    progress = 0;
    if (UseResume)
    {
        $.ajax({
            async: false,
            type: 'GET',
            url: getStateURL,
            success: function (data) {
                if (data != null && data > 0)
                    start = data;
            }
        });
    }
    UploadFile(FileName, start, UploadURL, ProgressCallBack);

};

function UploadFile(TargetFile, startPart, UploadURL, ProgressCallBack) {

    // create array to store the buffer chunks
    var FileChunk = [];
    // the file object itself that we will work with
        var file = TargetFile;
        // set up other initial vars
        var MaxFileSizeByte = 64;
        var BufferChunkSize = MaxFileSizeByte * 1024;
        var ReadBuffer_Size = 1024;
        var FileStreamPos = 0;
        // set the initial chunk length
        var EndPos = BufferChunkSize;
        var Size = file.size;
        var PartCount = 0;
        var TotalParts = Math.floor(Size / BufferChunkSize);
        // add to the FileChunk array until we get to the end of the file
        TotalProg = TotalParts;
        while (FileStreamPos < Size) {
            var FilePartName = file.name + ".part_" + PartCount + "." + TotalParts;
            var chunk = file.slice(FileStreamPos, EndPos);
            // "slice" the file from the starting position/offset, to  the required length
            //FileChunk.push();
            FileStreamPos = EndPos; // jump by the amount read
            EndPos = FileStreamPos + BufferChunkSize; // set next chunk length

            PartCount++;
            // file name convention
            // send the file
            if (PartCount >= startPart)
            {
                startProg = startPart;
                UploadFileChunk(chunk, FilePartName, UploadURL, ProgressCallBack);
            }
        }
        WaitSec();
        while (PartCount < TotalParts) {
            PartCount++;
            var FilePartName = file.name + ".part_" + PartCount + "." + TotalParts;
            UploadFileChunk(null, FilePartName,url);
        }


}
function UploadFileChunk(Chunk, FileName, UploadURL, ProgressCallBack) {
    var FD = new FormData();
    FD.append('file', Chunk, FileName);
    $.ajax({
        type: "POST",
        url: UploadURL,
        contentType: false,
        processData: false,
        data: FD,
        success: function ()
        {
            //claculate progress
            if (startProg > 0 && CurrentProg == 0)
                CurrentProg = startProg;
            if (++CurrentProg > TotalProg) {
                CurrentProg = 1;
            }
            progress = Math.floor((CurrentProg / TotalProg) * 100);
            ProgressCallBack(progress);
            ////Set value to the Knob
            //$('#knob')
            //    .val(progress)
            //    .trigger('change');
        }
    });
}
function WaitSec() {
    window.setTimeout(partB, 2000);
}

function partB() {

}