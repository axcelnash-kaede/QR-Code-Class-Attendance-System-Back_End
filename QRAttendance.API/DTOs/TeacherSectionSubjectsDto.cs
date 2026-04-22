namespace QRAttendance.API.DTOs
{
    public class TeacherSectionSubjectsDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public List<TeacherSectionSubjectItemDto> Subjects { get; set; } = new();
    }
}